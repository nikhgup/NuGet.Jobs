﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Services.Configuration;
using NuGet.Services.Metadata.Catalog;
using NuGet.Services.Metadata.Catalog.Monitoring;

namespace Ng.Jobs
{
    /// <summary>
    /// Runs a <see cref="ValidationCollector"/> on the catalog.
    /// The purpose of this job is to queue newly added, modified, or deleted packages for the <see cref="MonitoringProcessor"/> to run validations on.
    /// </summary>
    public class Catalog2MonitoringJob : LoopingNgJob
    {
        private PackageValidatorContextEnqueuer _enqueuer; 

        public Catalog2MonitoringJob(ITelemetryService telemetryService, ILoggerFactory loggerFactory)
            : base(telemetryService, loggerFactory)
        {
        }

        protected override void Init(IDictionary<string, string> arguments, CancellationToken cancellationToken)
        {
            var gallery = arguments.GetOrThrow<string>(Arguments.Gallery);
            var index = arguments.GetOrThrow<string>(Arguments.Index);
            var source = arguments.GetOrThrow<string>(Arguments.Source);
            var verbose = arguments.GetOrDefault(Arguments.Verbose, false);

            CommandHelpers.AssertAzureStorage(arguments);

            var monitoringStorageFactory = CommandHelpers.CreateStorageFactory(arguments, verbose);

            var endpointConfiguration = CommandHelpers.GetEndpointConfiguration(arguments);

            var messageHandlerFactory = CommandHelpers.GetHttpMessageHandlerFactory(TelemetryService, verbose);

            var statusService = CommandHelpers.GetPackageMonitoringStatusService(arguments, monitoringStorageFactory, LoggerFactory);

            var queue = CommandHelpers.CreateStorageQueue<PackageValidatorContext>(arguments, PackageValidatorContext.Version);

            Logger.LogInformation(
                "CONFIG gallery: {Gallery} index: {Index} storage: {Storage} registration cursor uri: {RegistrationCursorUri} flat-container cursor uri: {FlatContainerCursorUri}",
                gallery, index, monitoringStorageFactory, endpointConfiguration.RegistrationCursorUri, endpointConfiguration.FlatContainerCursorUri);

            _enqueuer = ValidationFactory.CreatePackageValidatorContextEnqueuer(
                queue,
                source,
                monitoringStorageFactory,
                endpointConfiguration,
                TelemetryService,
                messageHandlerFactory,
                LoggerFactory);
        }

        protected override async Task RunInternalAsync(CancellationToken cancellationToken)
        {
            await _enqueuer.EnqueuePackageValidatorContexts(cancellationToken);
        }
    }
}
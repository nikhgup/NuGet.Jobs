﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Jobs.Extensions;
using NuGet.Services.Status;

namespace StatusAggregator.Export
{
    public class StatusExporter : IStatusExporter
    {
        private readonly IComponentExporter _componentExporter;
        private readonly IEventExporter _eventExporter;
        private readonly IStatusSerializer _serializer;

        private readonly ILogger<StatusExporter> _logger;

        public StatusExporter(
            IComponentExporter componentExporter,
            IEventExporter eventExporter,
            IStatusSerializer serializer,
            ILogger<StatusExporter> logger)
        {
            _componentExporter = componentExporter ?? throw new ArgumentNullException(nameof(componentExporter));
            _eventExporter = eventExporter ?? throw new ArgumentNullException(nameof(eventExporter));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ServiceStatus> Export()
        {
            using (_logger.Scope("Exporting service status."))
            {
                var rootComponent = _componentExporter.Export();
                var recentEvents = _eventExporter.Export();
                return _serializer.Serialize(rootComponent, recentEvents);
            }
        }
    }
}

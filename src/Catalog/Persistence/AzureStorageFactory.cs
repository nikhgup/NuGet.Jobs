﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.WindowsAzure.Storage;

namespace NuGet.Services.Metadata.Catalog.Persistence
{
    public class AzureStorageFactory : StorageFactory
    {
        private readonly CloudStorageAccount _account;
        private readonly string _containerName;
        private readonly string _path;
        private readonly Uri _differentBaseAddress = null;
        private readonly TimeSpan _maxExecutionTime;
        private readonly TimeSpan _serverTimeout;

        public AzureStorageFactory(CloudStorageAccount account,
                                   string containerName,
                                   TimeSpan maxExecutionTime,
                                   TimeSpan serverTimeout,
                                   string path = null,
                                   Uri baseAddress = null)
        {
            _account = account;
            _containerName = containerName;
            _path = null;
            _maxExecutionTime = maxExecutionTime;
            _serverTimeout = serverTimeout;

            if (path != null)
            {
                _path = path.Trim('/') + '/';
            }

            _differentBaseAddress = baseAddress;

            var blobEndpointBuilder = new UriBuilder(account.BlobEndpoint)
            {
                Scheme = "http", // Convert base address to http. 'https' can be used for communication but is not part of the names.
                Port = 80
            };

            if (baseAddress == null)
            {
                BaseAddress = new Uri(blobEndpointBuilder.Uri, containerName + "/" + _path ?? string.Empty);
            }
            else
            {
                Uri newAddress = baseAddress;

                if (path != null)
                {
                    newAddress = new Uri(baseAddress, path + "/");
                }

                BaseAddress = newAddress;
            }

            // Beautify the destination URL.
            blobEndpointBuilder.Scheme = "https";
            blobEndpointBuilder.Port = -1;

            DestinationAddress = new Uri(blobEndpointBuilder.Uri, containerName + "/" + _path ?? string.Empty);
        }

        public bool CompressContent { get; set; }

        public override Storage Create(string name = null)
        {
            string path = (_path == null) ? name : _path + name;

            path = (name == null) ? (_path == null ? String.Empty : _path.Trim('/')) : path;

            Uri newBase = _differentBaseAddress;

            if (newBase != null && !string.IsNullOrEmpty(name))
            {
                newBase = new Uri(_differentBaseAddress, name + "/");
            }

            return new AzureStorage(_account, _containerName, path, newBase, _maxExecutionTime, _serverTimeout)
                                   { Verbose = Verbose, CompressContent = CompressContent };
        }
    }
}
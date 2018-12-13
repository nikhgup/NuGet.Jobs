﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NuGet.Protocol.Catalog;
using NuGet.Services.Entities;
using NuGetGallery;
using Xunit.Abstractions;
using PackageDependency = NuGet.Protocol.Catalog.PackageDependency;

namespace NuGet.Services.AzureSearch.Support
{
    public static class Data
    {
        public const string PackageId = "WindowsAzure.Storage";
        public const string NormalizedVersion = "7.1.2-alpha";
        public const string FullVersion = "7.1.2-alpha+git";
        public static readonly DateTimeOffset DocumentLastUpdated = new DateTimeOffset(2018, 12, 14, 9, 30, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset CommitTimestamp = new DateTimeOffset(2018, 12, 13, 12, 30, 0, TimeSpan.Zero);
        public static readonly string CommitId = "6b9b24dd-7aec-48ae-afc1-2a117e3d50d1";

        public static void SetDocumentLastUpdated(ICommittedDocument document, ITestOutputHelper output)
        {
            var currentTimestamp = document.LastUpdatedDocument;
            output.WriteLine(
                $"The commited document has a generated {nameof(document.LastUpdatedDocument)} value of " +
                $"{currentTimestamp:O}. Replacing this value with {DocumentLastUpdated:O}.");
            document.LastUpdatedDocument = DocumentLastUpdated;
        }

        public static Package PackageEntity => new Package
        {
            FlattenedAuthors = "Microsoft",
            Copyright = "© Microsoft Corporation. All rights reserved.",
            Created = new DateTime(2017, 1, 1),
            Description = "Description.",
            FlattenedDependencies = "Microsoft.Data.OData:5.6.4:net40-client|Newtonsoft.Json:6.0.8:net40-client",
            Hash = "oMs9XKzRTsbnIpITcqZ5XAv1h2z6oyJ33+Z/PJx36iVikge/8wm5AORqAv7soKND3v5/0QWW9PQ0ktQuQu9aQQ==",
            HashAlgorithm = "SHA512",
            IconUrl = "http://go.microsoft.com/fwlink/?LinkID=288890",
            IsPrerelease = true,
            Language = "en-US",
            LastEdited = new DateTime(2017, 1, 2),
            LicenseUrl = "http://go.microsoft.com/fwlink/?LinkId=331471",
            Listed = true,
            MinClientVersion = "2.12",
            NormalizedVersion = "7.1.2-alpha",
            PackageFileSize = 3039254,
            ProjectUrl = "https://github.com/Azure/azure-storage-net",
            Published = new DateTime(2017, 1, 3),
            ReleaseNotes = "Release notes.",
            RequiresLicenseAcceptance = true,
            SemVerLevelKey = SemVerLevelKey.SemVer2,
            Summary = "Summary.",
            Tags = "Microsoft Azure Storage Table Blob File Queue Scalable windowsazureofficial",
            Title = "Windows Azure Storage",
            Version = "7.1.2.0-alpha+git",
        };

        public static PackageDetailsCatalogLeaf Leaf => new PackageDetailsCatalogLeaf
        {
            Authors = "Microsoft",
            CommitId = CommitId,
            CommitTimestamp = CommitTimestamp,
            Copyright = "© Microsoft Corporation. All rights reserved.",
            Created = new DateTimeOffset(new DateTime(2017, 1, 1), TimeSpan.Zero),
            Description = "Description.",
            DependencyGroups = new List<PackageDependencyGroup>
            {
                new PackageDependencyGroup
                {
                    TargetFramework = ".NETFramework4.0-Client",
                    Dependencies = new List<PackageDependency>
                    {
                        new PackageDependency
                        {
                            Id = "Microsoft.Data.OData",
                            Range = "[5.6.4, )",
                        },
                        new PackageDependency
                        {
                            Id = "Newtonsoft.Json",
                            Range = "[6.0.8, )",
                        },
                    },
                },
            },
            IconUrl = "http://go.microsoft.com/fwlink/?LinkID=288890",
            IsPrerelease = true,
            Language = "en-US",
            LastEdited = new DateTimeOffset(new DateTime(2017, 1, 2), TimeSpan.Zero),
            LicenseUrl = "http://go.microsoft.com/fwlink/?LinkId=331471",
            Listed = true,
            MinClientVersion = "2.12",
            PackageHash = "oMs9XKzRTsbnIpITcqZ5XAv1h2z6oyJ33+Z/PJx36iVikge/8wm5AORqAv7soKND3v5/0QWW9PQ0ktQuQu9aQQ==",
            PackageHashAlgorithm = "SHA512",
            PackageId = PackageId,
            PackageSize = 3039254,
            PackageVersion = FullVersion,
            ProjectUrl = "https://github.com/Azure/azure-storage-net",
            Published = new DateTimeOffset(new DateTime(2017, 1, 3), TimeSpan.Zero),
            ReleaseNotes = "Release notes.",
            RequireLicenseAgreement = true,
            Summary = "Summary.",
            Tags = new List<string> { "Microsoft", "Azure", "Storage", "Table", "Blob", "File", "Queue", "Scalable", "windowsazureofficial" },
            Title = "Windows Azure Storage",
            VerbatimVersion = "7.1.2.0-alpha+git",
        };
    }
}

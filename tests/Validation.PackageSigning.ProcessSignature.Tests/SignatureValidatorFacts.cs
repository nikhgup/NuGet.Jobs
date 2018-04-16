﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Common;
using NuGet.Jobs.Validation.PackageSigning.Messages;
using NuGet.Jobs.Validation.PackageSigning.ProcessSignature;
using NuGet.Jobs.Validation.PackageSigning.Storage;
using NuGet.Jobs.Validation.PackageSigning.Telemetry;
using NuGet.Jobs.Validation.Storage;
using NuGet.Packaging.Signing;
using NuGet.Services.Validation;
using NuGet.Services.Validation.Issues;
using NuGetGallery;
using Xunit;
using Xunit.Abstractions;

namespace Validation.PackageSigning.ProcessSignature.Tests
{
    public class SignatureValidatorFacts
    {
        public class ValidateAsync
        {
            private MemoryStream _packageStream;
            private readonly int _packageKey;
            private readonly SignatureValidationMessage _message;
            private readonly CancellationToken _cancellationToken;
            private readonly Mock<IPackageSigningStateService> _packageSigningStateService;
            private VerifySignaturesResult _mimialVerifyResult;
            private readonly Mock<IPackageSignatureVerifier> _mimimalPackageSignatureVerifier;
            private VerifySignaturesResult _fullVerifyResult;
            private readonly Mock<IPackageSignatureVerifier> _fullPackageSignatureVerifier;
            private readonly Mock<ISignaturePartsExtractor> _signaturePartsExtractor;
            private readonly Mock<IEntityRepository<Certificate>> _certificates;
            private readonly ILogger<SignatureValidator> _logger;
            private readonly Mock<IProcessorPackageFileService> _packageFileService;
            private readonly Uri _nupkgUri;
            private readonly SignatureValidator _target;
            private readonly Mock<ITelemetryService> _telemetryService;

            public ValidateAsync(ITestOutputHelper output)
            {
                _packageStream = TestResources.GetResourceStream(TestResources.UnsignedPackage);
                _packageKey = 42;
                _message = new SignatureValidationMessage(
                    "NuGet.Versioning",
                    "4.3.0",
                    new Uri("https://example/nuget.versioning.4.3.0.nupkg"),
                    new Guid("b777135f-1aac-4ec2-a3eb-1f64fe1880d5"));
                _cancellationToken = CancellationToken.None;

                _packageSigningStateService = new Mock<IPackageSigningStateService>();

                _mimialVerifyResult = new VerifySignaturesResult(true);
                _mimimalPackageSignatureVerifier = new Mock<IPackageSignatureVerifier>();
                _mimimalPackageSignatureVerifier
                    .Setup(x => x.VerifySignaturesAsync(It.IsAny<ISignedPackageReader>(), It.IsAny<CancellationToken>(), It.IsAny<Guid>()))
                    .ReturnsAsync(() => _mimialVerifyResult);

                _fullVerifyResult = new VerifySignaturesResult(true);
                _fullPackageSignatureVerifier = new Mock<IPackageSignatureVerifier>();
                _fullPackageSignatureVerifier
                    .Setup(x => x.VerifySignaturesAsync(It.IsAny<ISignedPackageReader>(), It.IsAny<CancellationToken>(), It.IsAny<Guid>()))
                    .ReturnsAsync(() => _fullVerifyResult);

                _signaturePartsExtractor = new Mock<ISignaturePartsExtractor>();
                _certificates = new Mock<IEntityRepository<Certificate>>();
                var loggerFactory = new LoggerFactory().AddXunit(output);
                _logger = loggerFactory.CreateLogger<SignatureValidator>();

                _certificates
                    .Setup(x => x.GetAll())
                    .Returns(Enumerable.Empty<Certificate>().AsQueryable());

                _packageFileService = new Mock<IProcessorPackageFileService>();
                _nupkgUri = new Uri("https://example-storage/TestProcessor/b777135f-1aac-4ec2-a3eb-1f64fe1880d5/nuget.versioning.4.3.0.nupkg");
                _packageFileService
                    .Setup(x => x.GetReadAndDeleteUriAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                    .ReturnsAsync(() => _nupkgUri);

                _telemetryService = new Mock<ITelemetryService>();

                _target = new SignatureValidator(
                    _packageSigningStateService.Object,
                    _mimimalPackageSignatureVerifier.Object,
                    _fullPackageSignatureVerifier.Object,
                    _signaturePartsExtractor.Object,
                    _packageFileService.Object,
                    _certificates.Object,
                    _telemetryService.Object,
                    _logger);
            }

            private void Validate(
                SignatureValidatorResult result,
                ValidationStatus validationStatus,
                PackageSigningStatus packageSigningStatus,
                Uri nupkgUri = null)
            {
                Assert.Equal(validationStatus, result.State);
                Assert.Equal(nupkgUri, result.NupkgUri);
                _packageSigningStateService.Verify(
                    x => x.SetPackageSigningState(
                        _packageKey,
                        _message.PackageId,
                        _message.PackageVersion,
                        packageSigningStatus),
                    Times.Once);
                _packageSigningStateService.Verify(
                    x => x.SetPackageSigningState(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<PackageSigningStatus>()),
                    Times.Once);

                if (validationStatus == ValidationStatus.Succeeded
                    && packageSigningStatus == PackageSigningStatus.Valid)
                {
                    _signaturePartsExtractor.Verify(
                        x => x.ExtractAsync(_packageKey, It.Is<PrimarySignature>(y => y != null), It.IsAny<CancellationToken>()),
                        Times.Once);
                    _signaturePartsExtractor.Verify(
                        x => x.ExtractAsync(It.IsAny<int>(), It.IsAny<PrimarySignature>(), It.IsAny<CancellationToken>()),
                        Times.Once);
                }
                else
                {
                    _signaturePartsExtractor.Verify(
                        x => x.ExtractAsync(It.IsAny<int>(), It.IsAny<PrimarySignature>(), It.IsAny<CancellationToken>()),
                        Times.Never);
                }
            }

            [Fact]
            public async Task RejectsZip64Packages()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.Zip64Package);

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                var issue = Assert.Single(result.Issues);
                Assert.Equal(ValidationIssueCode.PackageIsZip64, issue.IssueCode);
            }

            [Fact]
            public async Task AcceptsSignedPackagesWithKnownCertificates()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.SignedPackageLeaf1);
                ConfigureKnownSignedPackage(TestResources.Leaf1Thumbprint);

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Succeeded, PackageSigningStatus.Valid);
                Assert.Empty(result.Issues);
            }

            [Fact]
            public async Task RejectsSignedPackagesWithFailedMinimalVerifyResult()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.SignedPackageLeaf1);
                _mimialVerifyResult = new VerifySignaturesResult(valid: false);

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                Assert.Empty(result.Issues);
                _fullPackageSignatureVerifier.Verify(
                    x => x.VerifySignaturesAsync(It.IsAny<ISignedPackageReader>(), It.IsAny<CancellationToken>(), It.IsAny<Guid>()),
                    Times.Never);
            }

            [Fact]
            public async Task RejectsPackagesWithMimimalVerificationErrors()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.SignedPackageLeaf1);
                _mimialVerifyResult = new VerifySignaturesResult(
                    valid: false,
                    results: new[]
                    {
                        new InvalidSignaturePackageVerificationResult(
                            SignatureVerificationStatus.Illegal,
                            new[]
                            {
                                SignatureLog.Issue(
                                    fatal: true,
                                    code: NuGetLogCode.NU3000,
                                    message: "The package signature is invalid."),
                            })
                    });

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                Assert.Single(result.Issues);
                var issue = Assert.IsType<ClientSigningVerificationFailure>(result.Issues[0]);
                Assert.Equal("NU3000", issue.ClientCode);
                Assert.Equal("The package signature is invalid.", issue.ClientMessage);
            }

            [Fact]
            public async Task RejectsSignedPackagesWithKnownCertificatesButFailedFullVerifyResult()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.SignedPackageLeaf1);
                ConfigureKnownSignedPackage(TestResources.Leaf1Thumbprint);
                _fullVerifyResult = new VerifySignaturesResult(valid: false);

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                Assert.Empty(result.Issues);
            }

            [Fact]
            public async Task RejectsPackagesWithFullVerificationErrors()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.SignedPackageLeaf1);
                ConfigureKnownSignedPackage(TestResources.Leaf1Thumbprint);
                _fullVerifyResult = new VerifySignaturesResult(
                    valid: false,
                    results: new[]
                    {
                        new InvalidSignaturePackageVerificationResult(
                            SignatureVerificationStatus.Illegal,
                            new[]
                            {
                                SignatureLog.Issue(
                                    fatal: true,
                                    code: NuGetLogCode.NU3008,
                                    message: "The package integrity check failed."),
                                SignatureLog.Issue(
                                    fatal: false,
                                    code: NuGetLogCode.NU3016,
                                    message: "The package hash uses an unsupported hash algorithm."),
                                SignatureLog.Issue(
                                    fatal: true,
                                    code: NuGetLogCode.NU3000,
                                    message: "Some other thing happened."),
                            })
                    });

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                Assert.Equal(2, result.Issues.Count);
                var issue1 = Assert.IsType<ClientSigningVerificationFailure>(result.Issues[0]);
                Assert.Equal("NU3008", issue1.ClientCode);
                Assert.Equal("The package integrity check failed.", issue1.ClientMessage);
                var issue2 = Assert.IsType<ClientSigningVerificationFailure>(result.Issues[1]);
                Assert.Equal("NU3000", issue2.ClientCode);
                Assert.Equal("Some other thing happened.", issue2.ClientMessage);
            }

            [Fact]
            public async Task RejectsSignedPackagesWithUnknownCertificates()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.SignedPackageLeaf1);
                ConfigureKnownSignedPackage(TestResources.Leaf2Thumbprint);

                // Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                var issue = Assert.Single(result.Issues);
                Assert.Equal(ValidationIssueCode.PackageIsSigned, issue.IssueCode);
            }

            [Fact]
            public async Task DoesNotUploadPackageWhenValidationFailed()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.AuthorAndRepoSignedPackageLeaf1);

                // Arrange & Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Failed, PackageSigningStatus.Invalid);
                _packageFileService.Verify(
                    x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Stream>()),
                    Times.Never);
            }

            [Theory]
            [InlineData(TestResources.RepoSignedPackageLeaf1, PackageSigningStatus.Unsigned)]
            [InlineData(TestResources.AuthorAndRepoSignedPackageLeaf1, PackageSigningStatus.Valid)]
            public async Task StripsAndAcceptsPackagesWithRepositorySignatures(string resourceName, PackageSigningStatus signingStatus)
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(resourceName);
                ConfigureKnownSignedPackage(TestResources.Leaf1Thumbprint);

                Stream uploadedStream = null;
                _packageFileService
                    .Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Stream>()))
                    .Returns(Task.CompletedTask)
                    .Callback<string, string, Guid, Stream>((_, __, ___, s) => uploadedStream = s);

                // Arrange & Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Succeeded, signingStatus, _nupkgUri);
                Assert.Empty(result.Issues);
                _packageFileService.Verify(
                    x => x.SaveAsync(_message.PackageId, _message.PackageVersion, _message.ValidationId, It.IsAny<Stream>()),
                    Times.Once);
                _packageFileService.Verify(
                    x => x.GetReadAndDeleteUriAsync(_message.PackageId, _message.PackageVersion, _message.ValidationId),
                    Times.Once);
                Assert.IsType<FileStream>(uploadedStream);
                Assert.Throws<ObjectDisposedException>(() => uploadedStream.Length);
            }

            [Fact]
            public async Task AcceptsUnsignedPackages()
            {
                // Arrange
                _packageStream = TestResources.GetResourceStream(TestResources.UnsignedPackage);

                // Arrange & Act
                var result = await _target.ValidateAsync(
                    _packageKey,
                    _packageStream,
                    _message,
                    _cancellationToken);

                // Assert
                Validate(result, ValidationStatus.Succeeded, PackageSigningStatus.Unsigned);
                Assert.Empty(result.Issues);
            }

            private void ConfigureKnownSignedPackage(string thumbprint)
            {
                _certificates
                    .Setup(x => x.GetAll())
                    .Returns(new[] { new Certificate { Thumbprint = thumbprint } }.AsQueryable());
            }
        }
    }
}
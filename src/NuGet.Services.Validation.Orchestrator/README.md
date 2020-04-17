The job that orchestrates the validation pipeline.

# Background

Every package has to go through a set of validators (or processors) before it gets published.

Validators abstracts single step of the pipeline that usually checks if package conforms to some requirement (but in general,
free to do anything). Validators cannot modify package.
Example: if a package author set up signing certificate in their NuGet account, validator can check if package is properly signed with
that certificate.

Processors are validators (so whenever we talk about validators, we mean both validators and processors) that can modify packages.
Example: repository signing processor takes original package and adds
a repository signature to it thus modifying the package.
The ability to modify packages adds some restrictions to processors: while several validators can execute in parallel, processors
have to run exclusively for a given package.

At any moment each validator can be in one of the following [states](https://github.com/NuGet/ServerCommon/blob/master/src/NuGet.Services.Contracts/Validation/ValidationStatus.cs):
* Not started;
* Incomplete;
* Succeeded;
* Failed.

Validators are organized into a graph using configuration file. Orchestrator walks each package through that graph running
whatever it can in parallel.
Each validator can have 0 or more prerequisite validators specified. Validator will not start unless all of its
prerequisites finish successfully.

Single set of validators for a single package make a validation set. A package can have any number of validation sets (they can run concurrently).
The first validation set that has all its validators succeed causes that package to get published. If any validator fails, processing of the whole
set stops.

# Configuration

## Command line arguments

```
NuGet.Services.Validation.Orchestrator.exe -Configuration <configuration_filename> [-InstanceName <instance_name>] [-InstrumentationKey <AI_instrumentation_key>] [-HeartbeatIntervalSeconds <seconds>]
```

* `-Configuration <configuration_filename>` - the name of the service configuration file;
* `-InstanceName <instance_name>` - optional name of the instance used in the logs. Will
	appear in `InstanceName` property of `customDimensions` object in AI. If not specified
	will use `NuGet.Services.Validation.Orchestrator`.
* `-InstrumentationKey <AI_instrumentation_key>` - AI instrumentation key to send logs to.
	If not specified will only log to console.
* `-HeartbeatIntervalSeconds <seconds>` - optional heartbeat send interval. If specified
	will override the default AI setting.

## Configuration file

Configuration file is a JSON file with the following properties:

### `Configuration`
### `RunnerConfiguration`
### `GalleryDb`
### `ValidationDb`
### `PackageSigning`
### `PackageCertificates`
### `ScanAndSign`
### `ServiceBus`
### `FlatContainer`
### `Email`
### `FeatureFlags`
### `Leases`
### `PackageDownloadTimeout`
### `KeyVault_VaultName`
### `KeyVaultUseManagedIdentity`

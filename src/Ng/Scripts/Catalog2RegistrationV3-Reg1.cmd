@echo OFF
	
cd Ng

:Top
	echo "Starting job - #{Jobs.catalog2registrationv3reg1.Title}"

	title #{Jobs.catalog2registrationv3reg1.Title}

    start /w Ng.exe catalog2registration -source #{Jobs.common.v3.Source} -contentBaseAddress #{Jobs.catalog2registrationv3reg1.ContentBaseAddress} -storageBaseAddress #{Jobs.catalog2registrationv3reg1.StorageBaseAddress} -storageType azure -storageAccountName #{Jobs.common.v3.c2r.StorageAccountName} -storageKeyValue #{Jobs.common.v3.c2r.StorageAccountKey} -storageContainer #{Jobs.catalog2registrationv3reg1.StorageContainer} -useCompressedStorage #{Jobs.catalog2registrationv3reg1.UseCompressedStorage} -compressedStorageBaseAddress #{Jobs.catalog2registrationv3reg1.StorageBaseAddressCompressed} -compressedStorageAccountName #{Jobs.common.v3.c2r.StorageAccountName} -compressedStorageKeyValue #{Jobs.common.v3.c2r.StorageAccountKey} -compressedStorageContainer #{Jobs.catalog2registrationv3reg1.StorageContainerCompressed} -instrumentationkey #{Jobs.common.v3.Logging.InstrumentationKey} -vaultName #{Deployment.Azure.KeyVault.VaultName} -clientId #{Deployment.Azure.KeyVault.ClientId} -certificateThumbprint #{Deployment.Azure.KeyVault.CertificateThumbprint} -verbose true -interval #{Jobs.common.v3.Interval}

	echo "Finished #{Jobs.catalog2registrationv3reg1.Title}"

	goto Top
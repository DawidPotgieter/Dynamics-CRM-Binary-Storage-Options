using BinaryStorageOptions.Configuration;
using BinaryStorageOptions.Providers;
using BinaryStorageOptions.Proxy.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BinaryStorageOptions.Proxy.Providers
{
	public static class Factory
	{
		public static IStorageProvider GetStorageProvider(IConfigurationProvider configurationProvider)
		{
			IStorageProvider storageProvider = null;
			switch (configurationProvider.StorageProviderType)
			{
				case StorageProviderType.AzureBlob:
					storageProvider = new AzureBlobStorageProvider((AzureBlobStorageConfiguration)configurationProvider.Configuration);
					break;
				case StorageProviderType.AzureFile:
					storageProvider = new AzureFileStorageProvider((AzureFileStorageConfiguration)configurationProvider.Configuration);
					break;
				case StorageProviderType.FileSystem:
					storageProvider = new FileSystemStorageProvider((FileSystemStorageConfiguration)configurationProvider.Configuration);
					break;
			}
			return storageProvider;
		}
	}
}
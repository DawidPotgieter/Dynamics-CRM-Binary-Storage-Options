using BinaryStorageOptions.Configuration;

namespace BinaryStorageOptions.Providers
{
	public static class Factory
	{
		public static IBinaryStorageProvider GetStorageProvider(IConfigurationProvider configuration)
		{
			IBinaryStorageProvider storageProvider = null;
			switch (configuration.StorageProviderType)
			{
				case BinaryStorageProviderType.AzureBlob:
					storageProvider = new AzureBlobBinaryStorageProvider((AzureBlobStorageConfiguration)configuration.Configuration);
					break;
				case BinaryStorageProviderType.AzureFile:
					storageProvider = new AzureFileBinaryStorageProvider((AzureFileStorageConfiguration)configuration.Configuration);
					break;
			}
			return storageProvider;
		}
	}
}

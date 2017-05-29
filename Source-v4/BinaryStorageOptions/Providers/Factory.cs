using BinaryStorageOptions.Configuration;

namespace BinaryStorageOptions.Providers
{
	public static class Factory
	{
		public static IBinaryStorageProvider GetStorageProvider(IConfigurationProvider configurationProvider)
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
			}
			return new BinaryStorageProvider(configurationProvider, storageProvider);
		}

		public static ICompressionProvider GetCompressionProvider(CompressionProviderType providerType, IConfigurationProvider configurationProvider)
		{
			ICompressionProvider provider = new PassThroughCompressionProvider();
			switch (providerType)
			{
				case CompressionProviderType.Zip:
					provider = new ZipCompressionProvider(configurationProvider);
					break;
			}
			return provider;
		}

		public static IEncryptionProvider GetEncryptionProvider(EncryptionProviderType providerType, IConfigurationProvider configurationProvider)
		{
			IEncryptionProvider provider = new PassThroughEncryptionProvider();
			switch (providerType)
			{
				case EncryptionProviderType.AES256:
					provider = new AES256EncryptionProvider(configurationProvider);
					break;
			}
			return provider;
		}
	}
}

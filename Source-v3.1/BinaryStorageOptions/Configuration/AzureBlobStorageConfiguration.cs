namespace BinaryStorageOptions.Configuration
{
	public class AzureBlobStorageConfiguration : IConfiguration
	{
		private IConfigurationProvider configurationProvider;
		private string entityType;

		public AzureBlobStorageConfiguration(IConfigurationProvider configurationProvider, string entityType)
		{
			this.configurationProvider = configurationProvider;
			this.entityType = entityType;
		}

		public string StorageAccount
		{
			get
			{
				return configurationProvider.GetSettingValue("StorageAccount");
			}
		}

		public string StorageKey
		{
			get
			{
				return configurationProvider.GetSettingValue("StorageKey");
			}
		}

		public string Container
		{
			get
			{
				return configurationProvider.GetSettingValue(entityType + "Container");
			}
		}
	}
}

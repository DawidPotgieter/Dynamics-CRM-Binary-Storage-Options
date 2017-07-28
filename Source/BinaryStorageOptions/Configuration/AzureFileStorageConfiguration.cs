namespace BinaryStorageOptions.Configuration
{
	public class AzureFileStorageConfiguration : IConfiguration
	{
		private IConfigurationProvider configurationProvider;
		private string entityType;

		public AzureFileStorageConfiguration(IConfigurationProvider configurationProvider, string entityType)
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

		public string Share
		{
			get
			{
				string value = null;
				try
				{
					value = configurationProvider.GetSettingValue(entityType + "Root");
				}
				catch { }
				//For backwards compatibility
				if (!string.IsNullOrWhiteSpace(value)) return value;
				return configurationProvider.GetSettingValue(entityType + "Share");
			}
		}

		/// <summary>
		/// Empty or in the format parentdirectory/childdirectory (no start or end slashes)
		/// </summary>
		public string Folder
		{
			get
			{
				return configurationProvider.GetSettingValue(entityType + "Folder");
			}
		}
	}
}

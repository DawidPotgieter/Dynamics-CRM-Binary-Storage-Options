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
				string value = null;
				try
				{
					value = configurationProvider.GetSettingValue(entityType + "Root");
				}
				catch { }
				//For backwards compatibility
				if (!string.IsNullOrWhiteSpace(value)) return value;
				return configurationProvider.GetSettingValue(entityType + "Container");
			}
		}

		private string folder = null;
		/// <summary>
		/// Empty or in the format parentdirectory/childdirectory (no start or end slashes)
		/// </summary>
		public string Folder
		{
			get
			{
				if (folder == null)
				{
					folder = string.Empty;
					try
					{
						folder = configurationProvider.GetSettingValue(entityType + "Folder");
					}
					catch { }
				}
				return folder;
			}
		}
	}
}

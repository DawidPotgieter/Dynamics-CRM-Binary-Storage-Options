namespace BinaryStorageOptions.Configuration
{
	public class ProxyStorageConfiguration : IConfiguration
	{
		private IConfigurationProvider configurationProvider;
		private string entityType;

		public ProxyStorageConfiguration(IConfigurationProvider configurationProvider, string entityType)
		{
			this.configurationProvider = configurationProvider;
			this.entityType = entityType;
		}

		public string Username
		{
			get
			{
				return configurationProvider.GetSettingValue("ProxyUsername");
			}
		}

		public string Password
		{
			get
			{
				return configurationProvider.GetSettingValue("ProxyPassword");
			}
		}

		public string Uri
		{
			get
			{
				return configurationProvider.GetSettingValue("ProxyUri");
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

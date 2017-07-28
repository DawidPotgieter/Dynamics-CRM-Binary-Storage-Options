using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BinaryStorageOptions.Proxy.Configuration
{
	public class FileSystemStorageConfiguration : IConfiguration
	{
		private IConfigurationProvider configurationProvider;
		private string entityType;

		public FileSystemStorageConfiguration(IConfigurationProvider configurationProvider, string entityType)
		{
			this.configurationProvider = configurationProvider;
			this.entityType = entityType;
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
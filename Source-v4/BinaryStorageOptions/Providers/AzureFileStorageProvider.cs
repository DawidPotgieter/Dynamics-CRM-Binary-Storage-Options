using Azure.Storage;
using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinaryStorageOptions.Providers
{
	public class AzureFileStorageProvider : IStorageProvider
	{
		private const string MetaDataPrefix = "bso_";

		AzureFileStorageConfiguration configuration;
		FileStorageHelper fileStorageHelper;

		public AzureFileStorageProvider(AzureFileStorageConfiguration configuration)
		{
			this.configuration = configuration;
			fileStorageHelper = new FileStorageHelper(configuration.StorageAccount, configuration.EndpointSuffix, configuration.StorageKey, configuration.IsSasToken);
		}

		public List<string> GetFileNames()
		{
			return fileStorageHelper.ListFilenames(configuration.Share, configuration.Folder);
		}

		public bool Create(Guid id, string filename, byte[] data, Dictionary<string, string> metaData = null)
		{
			Dictionary<string, string> alteredMetaData = metaData;
			if (metaData != null)
			{
				alteredMetaData = metaData.ToDictionary(m => MetaDataPrefix + m.Key, m => m.Value);
			}
			return fileStorageHelper.PutFile(configuration.Share, configuration.Folder, GetFullFilename(id, filename), data, alteredMetaData);
		}

		public bool Delete(Guid id, string filename)
		{
			return fileStorageHelper.DeleteFile(configuration.Share, configuration.Folder, GetFullFilename(id, filename));
		}

		public byte[] Read(Guid id, string filename, out Dictionary<string, string> metaData)
		{
			metaData = null;
			Dictionary<string, string> storeMetaData = null;
			byte[] data = fileStorageHelper.GetFile(configuration.Share, configuration.Folder, GetFullFilename(id, filename), out storeMetaData);
			if (storeMetaData != null)
			{
				metaData = storeMetaData.ToDictionary(m => m.Key.Replace(MetaDataPrefix, ""), m => m.Value);
			}
			return data;
		}

		public Dictionary<string, string> GetMetaData(Guid id, string filename, bool customOnly = true)
		{
			return fileStorageHelper.GetMetaData(configuration.Share, configuration.Folder, GetFullFilename(id, filename), customOnly);
		}

		public int GetFileSize(Guid id, string filename)
		{
			return fileStorageHelper.GetFileSize(configuration.Share, configuration.Folder, GetFullFilename(id, filename));
		}

		private string GetFullFilename(Guid id, string filename)
		{
			return string.Format("{0}_{1}", id.ToString(), filename);
		}
	}
}

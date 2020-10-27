using Azure.Storage;
using BinaryStorageOptions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BinaryStorageOptions.Providers
{
	public class AzureBlobStorageProvider : IStorageProvider
	{
		private const string MetaDataPrefix = "bso_";

		AzureBlobStorageConfiguration configuration;
		BlobStorageHelper blobStorageHelper;

		public AzureBlobStorageProvider(AzureBlobStorageConfiguration configuration)
		{
			this.configuration = configuration;
			blobStorageHelper = new BlobStorageHelper(configuration.StorageAccount, configuration.EndpointSuffix, configuration.StorageKey);
		}

		public List<string> GetFileNames()
		{
			string replace = configuration.Folder + "/";
			return blobStorageHelper.ListBlobNames(configuration.Container).Where(n => n.StartsWith(replace)).Select(n => n.Replace(replace, "")).ToList();
		}

		public bool Create(Guid id, string filename, byte[] data, Dictionary<string, string> metaData = null)
		{
			Dictionary<string, string> alteredMetaData = metaData;
			if (metaData != null)
			{
				alteredMetaData = metaData.ToDictionary(m => MetaDataPrefix + m.Key, m => m.Value);
			}
			return blobStorageHelper.PutBlob(configuration.Container, GetFullFilename(id, filename), data, alteredMetaData);
		}

		public bool Delete(Guid id, string filename)
		{
			return blobStorageHelper.DeleteBlob(configuration.Container, GetFullFilename(id, filename));
		}

		public byte[] Read(Guid id, string filename, out Dictionary<string, string> metaData)
		{
			metaData = null;
			Dictionary<string, string> storeMetaData = null;
			byte[] data = blobStorageHelper.GetBlob(configuration.Container, GetFullFilename(id, filename), out storeMetaData);
			if (storeMetaData != null)
			{
				metaData = storeMetaData.ToDictionary(m => m.Key.Replace(MetaDataPrefix, ""), m => m.Value);
			}
			return data;
		}

		public Dictionary<string, string> GetMetaData(Guid id, string filename, bool customOnly = true)
		{
			return blobStorageHelper.GetMetaData(configuration.Container, GetFullFilename(id, filename), customOnly);
		}

		public int GetFileSize(Guid id, string filename)
		{
			return blobStorageHelper.GetBlobSize(configuration.Container, GetFullFilename(id, filename));
		}

		private string GetFullFilename(Guid id, string filename)
		{
			return string.Format("{0}/{1}_{2}", configuration.Folder, id.ToString(), filename).TrimStart('/');
		}
	}
}

using Azure.Storage;
using BinaryStorageOptions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinaryStorageOptions.Providers
{
	public class AzureFileBinaryStorageProvider : IBinaryStorageProvider
	{
		AzureFileStorageConfiguration configuration;
		FileStorageHelper fileStorageHelper;

		public AzureFileBinaryStorageProvider(AzureFileStorageConfiguration configuration)
		{
			this.configuration = configuration;
			fileStorageHelper = new FileStorageHelper(configuration.StorageAccount, configuration.StorageKey);
		}

		public List<string> GetFileNames()
		{
			return fileStorageHelper.ListFilenames(configuration.Share, configuration.Folder);
		}

		public bool Create(Guid id, string filename, byte[] data)
		{
			return fileStorageHelper.PutFile(configuration.Share, configuration.Folder, GetFullFilename(id, filename), data);
		}

		public bool Delete(Guid id)
		{
			return fileStorageHelper.DeleteFilePrefixed(configuration.Share, configuration.Folder, id.ToString());
		}

		public bool Delete(string fileName)
		{
			return fileStorageHelper.DeleteFile(configuration.Share, configuration.Folder, fileName);
		}

		public byte[] Read(Guid id, string filename)
		{
			return fileStorageHelper.GetFile(configuration.Share, configuration.Folder, GetFullFilename(id, filename));
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

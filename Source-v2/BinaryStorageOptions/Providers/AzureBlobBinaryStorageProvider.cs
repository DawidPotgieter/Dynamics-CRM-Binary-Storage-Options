using Azure.Storage;
using BinaryStorageOptions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BinaryStorageOptions.Providers
{
	public class AzureBlobBinaryStorageProvider : IBinaryStorageProvider
	{
		AzureBlobStorageConfiguration configuration;
		BlobStorageHelper blobStorageHelper;

		public AzureBlobBinaryStorageProvider(AzureBlobStorageConfiguration configuration)
		{
			this.configuration = configuration;
			blobStorageHelper = new BlobStorageHelper(configuration.StorageAccount, configuration.StorageKey);
		}

		public List<string> GetFileNames()
		{
			return blobStorageHelper.ListBlobNames(configuration.Container);
		}

		public bool Create(Guid id, string filename, byte[] data)
		{
			return blobStorageHelper.PutBlob(configuration.Container, GetFullFilename(id, filename), data);
		}

		public bool Delete(Guid id)
		{
			return blobStorageHelper.DeleteBlobPrefixed(configuration.Container, id.ToString());
		}

		public bool Delete(string fileName)
		{
			return blobStorageHelper.DeleteBlob(configuration.Container, fileName);
		}

		public byte[] Read(Guid id, string filename)
		{
			return blobStorageHelper.GetBlob(configuration.Container, GetFullFilename(id, filename));
		}

		public int GetFileSize(Guid id, string filename)
		{
			return blobStorageHelper.GetBlobSize(configuration.Container, GetFullFilename(id, filename));
		}

		private string GetFullFilename(Guid id, string filename)
		{
			return string.Format("{0}_{1}", id.ToString(), filename);
		}
	}
}

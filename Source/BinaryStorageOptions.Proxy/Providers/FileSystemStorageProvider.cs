using BinaryStorageOptions.Providers;
using BinaryStorageOptions.Proxy.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BinaryStorageOptions.Proxy.Providers
{
	public class FileSystemStorageProvider : IStorageProvider
	{
		private const string MetaDataFileExtension = ".metadata";

		FileSystemStorageConfiguration configuration;

		public FileSystemStorageProvider(FileSystemStorageConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public List<string> GetFileNames()
		{
			return Directory.EnumerateFiles(configuration.Folder).Where(fn => Path.GetExtension(fn) != MetaDataFileExtension).Select(fn => fn.Replace(configuration.Folder + "\\", "")).ToList();
		}

		public bool Create(Guid id, string filename, byte[] data, Dictionary<string, string> metaData = null)
		{
			Dictionary<string, string> alteredMetaData = metaData;
			if (metaData != null && metaData.Count > 0)
			{
				string metaDataPath = GetFullPath(id, filename, MetaDataFileExtension);
				if (File.Exists(metaDataPath))
				{
					File.SetAttributes(metaDataPath, FileAttributes.Normal);
				}
				File.WriteAllText(metaDataPath, JsonConvert.SerializeObject(metaData));
				File.SetAttributes(metaDataPath, FileAttributes.Hidden | FileAttributes.ReadOnly);
			}
			File.WriteAllBytes(GetFullPath(id, filename), data);
			return true;
		}

		public bool Delete(Guid id, string filename)
		{
			string metaDataPath = GetFullPath(id, filename, MetaDataFileExtension);
			if (File.Exists(metaDataPath))
			{
				File.SetAttributes(metaDataPath, FileAttributes.Normal);
				File.Delete(metaDataPath);
			}
			File.Delete(GetFullPath(id, filename));
			return true;
		}

		public byte[] Read(Guid id, string filename, out Dictionary<string, string> metaData)
		{
			metaData = new Dictionary<string, string>();
			if (File.Exists(GetFullPath(id, filename, MetaDataFileExtension)))
			{
				metaData = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(GetFullPath(id, filename, MetaDataFileExtension)));
			}
			return File.ReadAllBytes(GetFullPath(id, filename));
		}

		public Dictionary<string, string> GetMetaData(Guid id, string filename, bool customOnly = true)
		{
			if (File.Exists(GetFullPath(id, filename, MetaDataFileExtension)))
			{
				return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(GetFullPath(id, filename, MetaDataFileExtension)));
			}
			return new Dictionary<string, string>();
		}

		public int GetFileSize(Guid id, string filename)
		{
			return (int)new FileInfo(GetFullPath(id, filename)).Length;
		}

		private string GetFullFilename(Guid id, string filename)
		{
			return string.Format("{0}_{1}", id.ToString(), filename);
		}

		private string GetFullPath(Guid id, string filename, string extension = null)
		{
			if (extension == null)
			{
				return Path.Combine(configuration.Folder, GetFullFilename(id, filename));
			}
			return Path.Combine(configuration.Folder, GetFullFilename(id, filename)) + extension;
		}
	}
}
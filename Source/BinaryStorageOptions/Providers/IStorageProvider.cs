using System;
using System.Collections.Generic;

namespace BinaryStorageOptions.Providers
{
	public interface IStorageProvider
	{
		List<string> GetFileNames();
		bool Create(Guid id, string filename, byte[] data, Dictionary<string, string> metaData = null);
		bool Delete(Guid id, string filename);
		byte[] Read(Guid id, string filename, out Dictionary<string, string> metaData);
		Dictionary<string, string> GetMetaData(Guid id, string filename, bool customOnly = true);
		int GetFileSize(Guid id, string filename);
	}
}

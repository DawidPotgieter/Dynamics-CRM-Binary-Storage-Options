using System;
using System.Collections.Generic;

namespace BinaryStorageOptions.Providers
{
	public interface IBinaryStorageProvider
	{
		List<string> GetFileNames();
		bool Create(Guid id, string filename, byte[] data);
		bool Delete(Guid id);
		bool Delete(string fileName);
		byte[] Read(Guid id, string filename);
		int GetFileSize(Guid id, string filename);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Proxy.Models
{
	public class FileWrapper
	{
		public Dictionary<string,string> MetaData { get; set; }
		public byte[] Data { get; set; }
	}
}

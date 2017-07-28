using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public class PassThroughCompressionProvider : ICompressionProvider
	{
		public byte[] Compress(byte[] data)
		{
			return data;
		}

		public byte[] Decompress(byte[] data)
		{
			return data;
		}
	}
}

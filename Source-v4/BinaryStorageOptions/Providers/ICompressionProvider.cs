using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions.Providers
{
	public interface ICompressionProvider
	{
		byte[] Compress(byte[] data);
		byte[] Decompress(byte[] data);
	}
}

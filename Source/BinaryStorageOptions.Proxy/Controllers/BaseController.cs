using BinaryStorageOptions.Configuration;
using BinaryStorageOptions.Providers;
using BinaryStorageOptions.Proxy.BasicAuthenticationModule;
using BinaryStorageOptions.Proxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BinaryStorageOptions.Proxy.Controllers
{
	[Authorize]
	//[HardCodedBasicAuthentication]
	public abstract class BaseController : ApiController
	{
		private IStorageProvider storageProvider;

		public BaseController(IConfigurationProvider configProvider)
		{
			storageProvider = Providers.Factory.GetStorageProvider(configProvider);
		}

		[HttpGet]
		[Route("GetFileNames")]
		public IEnumerable<string> GetFileNames()
		{
			return storageProvider.GetFileNames();
		}

		[HttpGet]
		[Route("Read")]
		public FileWrapper Read(Guid id, string filename)
		{
			byte[] data = storageProvider.Read(id, filename, out Dictionary<string, string> metaData);
			return new FileWrapper
			{
				Data = data,
				MetaData = metaData,
			};
		}

		[HttpGet]
		[Route("GetMetaData")]
		public Dictionary<string, string> GetMetaData(Guid id, string filename, bool customOnly = true)
		{
			return storageProvider.GetMetaData(id, filename, customOnly);
		}

		[HttpGet]
		[Route("GetFileSize")]
		public int GetFileSize(Guid id, string filename)
		{
			return storageProvider.GetFileSize(id, filename);
		}

		[HttpPut]
		[Route("Create")]
		public async Task<bool> Create(Guid id, string filename, [FromUri] Dictionary<string, string> metaData)
		{
			byte[] data = await Request.Content.ReadAsByteArrayAsync();
			return storageProvider.Create(id, filename, data, metaData);
		}

		[HttpDelete]
		[Route("Delete")]
		public bool Delete(Guid id, string filename)
		{
			return storageProvider.Delete(id, filename);
		}
	}
}
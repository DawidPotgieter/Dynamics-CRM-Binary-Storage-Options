using BinaryStorageOptions.Configuration;
using BinaryStorageOptions.Providers;
using BinaryStorageOptions.Proxy.Configuration;
using BinaryStorageOptions.Proxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BinaryStorageOptions.Proxy.Controllers
{
	[RoutePrefix("activitymimeattachments")]
	public class AttachmentsController : BaseController
	{
		public AttachmentsController() 
			: base(new AppSettingConfigurationProvider(CrmConstants.AttachmentEntityName))
		{
		}
	}
}
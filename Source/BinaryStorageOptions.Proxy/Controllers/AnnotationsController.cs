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
	[RoutePrefix("Annotations")]
	public class AnnotationsController : BaseController
	{
		public AnnotationsController() 
			: base(new AppSettingConfigurationProvider(CrmConstants.AnnotationEntityName))
		{
		}
	}
}
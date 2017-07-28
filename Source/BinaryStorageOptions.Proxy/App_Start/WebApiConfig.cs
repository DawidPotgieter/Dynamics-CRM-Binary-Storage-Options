using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace BinaryStorageOptions.Proxy
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API routes
			config.MapHttpAttributeRoutes(new CustomDirectRouteProvider());

			// Other Web API configuration not shown.
		}
	}

	public class CustomDirectRouteProvider : DefaultDirectRouteProvider
	{
		protected override IReadOnlyList<IDirectRouteFactory>
		GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
		{
			// inherit route attributes decorated on base class controller's actions
			return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>
			(inherit: true);
		}
	}
}
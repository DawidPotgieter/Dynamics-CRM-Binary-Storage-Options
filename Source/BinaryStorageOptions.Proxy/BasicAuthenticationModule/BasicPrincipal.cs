using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace BinaryStorageOptions.Proxy.BasicAuthenticationModule
{
	public class BasicPrincipal : IPrincipal
	{
		public BasicPrincipal(IIdentity identity)
		{
			this.identity = identity;
		}

		private IIdentity identity;
		public IIdentity Identity
		{
			get
			{
				return identity;
			}
		}

		public bool IsInRole(string role)
		{
			throw new NotImplementedException();
		}
	}
}
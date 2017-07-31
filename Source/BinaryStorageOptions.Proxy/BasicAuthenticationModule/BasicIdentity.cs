using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace BinaryStorageOptions.Proxy.BasicAuthenticationModule
{
	internal class BasicIdentity : IIdentity
	{
		private BasicIdentity(string username, bool isAuthenticated)
		{
			this.isAuthenticated = isAuthenticated;
			this.username = username;
		}

		public static Task<BasicIdentity> Create(string username, bool isAuthenticated)
		{
			return Task<BasicIdentity>.Factory.StartNew(() => { return new BasicIdentity(username, isAuthenticated); });
		}

		public string AuthenticationType
		{
			get
			{
				return "Basic";
			}
		}

		private bool isAuthenticated;
		public bool IsAuthenticated
		{
			get
			{
				return isAuthenticated;
			}
		}

		private string username;
		public string Name
		{
			get
			{
				return username;				
			}
		}
	}
}
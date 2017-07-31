using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BinaryStorageOptions.Proxy.BasicAuthenticationModule
{
	public class HardCodedBasicAuthenticationAttribute : BasicAuthenticationAttribute
	{
		protected override async Task<IPrincipal> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
		{
			if (username == "DoNotUseThis" && password == "ModuleInProduction")
			{
				BasicIdentity identity = await BasicIdentity.Create(username, true);
				return new BasicPrincipal(identity);
			}
			return null;
		}
	}
}
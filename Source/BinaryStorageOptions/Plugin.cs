using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryStorageOptions
{
	public partial class Plugin : IPlugin
	{
		private readonly string unsecurePluginStepConfiguration;
		private readonly string securePluginStepConfiguration;

		public Plugin(string unsecurePluginStepConfiguration, string securePluginStepConfiguration)
		{
			this.unsecurePluginStepConfiguration = unsecurePluginStepConfiguration;
			this.securePluginStepConfiguration = securePluginStepConfiguration;
		}

		public void Execute(IServiceProvider serviceProvider)
		{
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

			switch (context.MessageName)
			{
				case MessageName.Create:
					Create(serviceProvider);
					break;
				case MessageName.Delete:
					Delete(serviceProvider);
					break;
				case MessageName.Retrieve:
				case MessageName.RetrieveMultiple:
					Retrieve(serviceProvider);
					break;
				case MessageName.Update:
					Update(serviceProvider);
					break;
			}
		}
	}
}

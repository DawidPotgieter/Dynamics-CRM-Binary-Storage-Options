using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Migrate
{
	public partial class Main : Form
	{
		private AuthenticationCredentials authCredentials;
		private OrganizationServiceProxy proxy;
		private IServiceManagement<IOrganizationService> serviceManagement;
		private ConcurrentQueue<Guid> annotationsToMigrate;
		private ConcurrentQueue<Guid> attachmentsToMigrate;
		private bool shouldCancel;
		private bool isBusy;

		public Main()
		{
			InitializeComponent();
		}

		private void Main_Load(object sender, EventArgs e)
		{
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				authGroup.Enabled = false;
				txtOrganizationServiceUrl.Enabled = false;
				serviceManagement = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(new Uri(txtOrganizationServiceUrl.Text));
				authCredentials = new AuthenticationCredentials();
				if (IfdSelected)
				{
					authCredentials.ClientCredentials.UserName.UserName = txtUsername.Text;
					authCredentials.ClientCredentials.UserName.Password = txtPassword.Text;
					authCredentials = serviceManagement.Authenticate(authCredentials);
					proxy = new OrganizationServiceProxy(serviceManagement, authCredentials.SecurityTokenResponse);
				}
				else
				{
					authCredentials.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
					proxy = new OrganizationServiceProxy(serviceManagement, authCredentials.ClientCredentials);
				}
				BinaryStorageOptions.Configuration.IConfigurationProvider annotationConfigProvider = 
					BinaryStorageOptions.Configuration.Factory.GetConfigurationProvider(proxy, BinaryStorageOptions.CrmConstants.AnnotationEntityName, GetUnsecurePluginConfiguration(proxy, BinaryStorageOptions.CrmConstants.AnnotationEntityName), GetSecurePluginConfiguration());
				if (annotationConfigProvider.StorageProviderType == BinaryStorageOptions.Providers.BinaryStorageProviderType.CrmDefault)
				{
					MessageBox.Show("The provider is set to 'CrmDefault'.  This means no migration will happen.", "Default Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				BinaryStorageOptions.Configuration.IConfigurationProvider attachmentConfigProvider = 
					BinaryStorageOptions.Configuration.Factory.GetConfigurationProvider(proxy, BinaryStorageOptions.CrmConstants.AttachmentEntityName, GetUnsecurePluginConfiguration(proxy, BinaryStorageOptions.CrmConstants.AttachmentEntityName), GetSecurePluginConfiguration());
				if (attachmentConfigProvider.StorageProviderType == BinaryStorageOptions.Providers.BinaryStorageProviderType.CrmDefault)
				{
					MessageBox.Show("The provider is set to 'CrmDefault'.  This means no migration will happen.", "Default Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				migrateGroup.Text = string.Format("Connected. External Storage Provider : {0}, {1}", GetExternalPath(annotationConfigProvider), GetExternalPath(attachmentConfigProvider));
				migrateGroup.Enabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Something very bad happened : " + ex.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
				authGroup.Enabled = true;
				txtOrganizationServiceUrl.Enabled = false;
			}
		}

		private string GetExternalPath(BinaryStorageOptions.Configuration.IConfigurationProvider configurationProvider)
		{
			string description = string.Format("{0} - ", configurationProvider.StorageProviderType);
			switch (configurationProvider.StorageProviderType)
			{
				case BinaryStorageOptions.Providers.BinaryStorageProviderType.AzureBlob:
					BinaryStorageOptions.Configuration.AzureBlobStorageConfiguration blobConfig = (BinaryStorageOptions.Configuration.AzureBlobStorageConfiguration)configurationProvider.Configuration;
					description += blobConfig.Container;
					break;
				case BinaryStorageOptions.Providers.BinaryStorageProviderType.AzureFile:
					BinaryStorageOptions.Configuration.AzureFileStorageConfiguration fileConfig = (BinaryStorageOptions.Configuration.AzureFileStorageConfiguration)configurationProvider.Configuration;
					description += (fileConfig.Share + "/" + fileConfig.Folder).TrimEnd('/');
					break;
			}
			return description;
		}

		private void rdoIFDAuth_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoIFDAuth.Checked)
			{
				txtUsername.Enabled = true;
				txtPassword.Enabled = true;
			}
			else
			{
				txtUsername.Enabled = false;
				txtPassword.Enabled = false;
			}
		}

		private void btnMigrate_Click(object sender, EventArgs e)
		{
			try
			{
				if (!isBusy)
				{
					if (!chkPluginStepsManage.Checked)
					{
						if (MessageBox.Show("You have unticked the 'Automatically Enable/Disable BinaryStorageOptions plugin' checkbox. You MUST disable the Retrieve/RetrieveMultiple messages on the plugin manually before running this.  If you don't bad things will happen! Are you sure you wish to proceed at this time?", "Plugin Steps", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
						{
							return;
						}
					}

					isBusy = true;
					migrateGroup.Controls.OfType<Control>().ToList().ForEach(c => c.Enabled = false);
					btnMigrate.Text = "Cancel";
					btnMigrate.Enabled = true;
					shouldCancel = false;
					if (rdoInbound.Checked)
					{
						Task.Run(() => Migrate(false, (int)udThreadCount.Value, (int)udWaitDelay.Value * 1000, chkMigrateAnnotations.Checked, chkMigrateAttachments.Checked, chkMoveAnnotations.Checked));
					}
					else
					{
						Task.Run(() => Migrate(true, (int)udThreadCount.Value, (int)udWaitDelay.Value * 1000, chkMigrateAnnotations.Checked, chkMigrateAttachments.Checked, chkMoveAnnotations.Checked));
					}
				}
				else
				{
					shouldCancel = true;
					btnMigrate.Text = "Stopping...";
					btnMigrate.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Something very bad happened : " + ex.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
				btnMigrate.Text = "Migrate";
				migrateGroup.Controls.OfType<Control>().ToList().ForEach(c => c.Enabled = true);
				isBusy = false;
				shouldCancel = false;
			}
		}

		private void Migrate(bool external, int threadCount, int waitTime, bool migrateAnnotations, bool migrateEmailAttachments, bool moveAnnotations)
		{
			try
			{
				if (chkPluginStepsManage.Checked)
				{
					SetPluginStepsStatus(proxy, true);
				}

				FillMigrationQueues(external, migrateAnnotations, migrateEmailAttachments);

				int totalCount = annotationsToMigrate.Count + attachmentsToMigrate.Count;

				if (totalCount > 0)
				{
					pgbProgress.InvokeEx(pb => { pb.Maximum = totalCount; pb.Value = 0; });
					lblProgress.InvokeEx(pl => pl.Text = Math.Round(((double)pgbProgress.Value / (double)totalCount) * 100, 1) + "%");
				}
				else
				{
					pgbProgress.InvokeEx(pb => pb.Value = pb.Maximum);
					lblProgress.InvokeEx(pl => pl.Text = "100%");
				}
				txtOutput.InvokeEx(tb => tb.Clear());

				while ((migrateAnnotations && annotationsToMigrate.Count > 0) || (migrateEmailAttachments && attachmentsToMigrate.Count > 0))
				{
					Notify("Starting Migration...");
					if (migrateAnnotations)
					{
						Parallel.For(0, annotationsToMigrate.Count, new ParallelOptions { MaxDegreeOfParallelism = threadCount },
							(i, state) =>
							{
								if (shouldCancel) state.Stop();
								Guid id;
								while (!annotationsToMigrate.TryDequeue(out id))
								{
									int sleepValue = new Random().Next(1000);
									Notify(string.Format("Waiting for {0} milliseconds...", sleepValue));
									System.Threading.Thread.Sleep(sleepValue);
								}
								if (!MigrateEntity(id, 
									BinaryStorageOptions.CrmConstants.AnnotationEntityName, 
									BinaryStorageOptions.GenericConstants.Constants[BinaryStorageOptions.CrmConstants.AnnotationEntityName][BinaryStorageOptions.GenericConstants.DocumentBodyAttributeKey], 
									BinaryStorageOptions.GenericConstants.Constants[BinaryStorageOptions.CrmConstants.AnnotationEntityName][BinaryStorageOptions.GenericConstants.FileNameAttributeKey], external,
									moveAnnotations))
								{
									NotifyError(string.Format("Migration of '{0}' with id '{1}' FAILED.", BinaryStorageOptions.CrmConstants.AnnotationEntityName, id));
								}
								pgbProgress.InvokeEx(pb => pgbProgress.Increment(1));
								lblProgress.InvokeEx(pl => pl.Text = Math.Round(((double)pgbProgress.Value / (double)totalCount) * 100, 1) + "%");
								//Notify(string.Format("Waiting for {0} milliseconds...", waitTime));
								System.Threading.Thread.Sleep(waitTime);
							});
					}
					if (migrateEmailAttachments)
					{
						if (shouldCancel) break;
						Parallel.For(0, attachmentsToMigrate.Count, new ParallelOptions { MaxDegreeOfParallelism = threadCount },
							(i, state) =>
							{
								if (shouldCancel) state.Stop();
								Notify(string.Format("Waiting for {0} seconds...", (int)udWaitDelay.Value));
								System.Threading.Thread.Sleep((int)udWaitDelay.Value);
								Guid id;
								while (!attachmentsToMigrate.TryDequeue(out id))
								{
									int sleepValue = new Random().Next(1000);
									Notify(string.Format("Waiting for {0} milliseconds...", sleepValue));
									System.Threading.Thread.Sleep(sleepValue);
								}
								if (!MigrateEntity(id, BinaryStorageOptions.CrmConstants.AttachmentEntityName, 
									BinaryStorageOptions.GenericConstants.Constants[BinaryStorageOptions.CrmConstants.AttachmentEntityName][BinaryStorageOptions.GenericConstants.DocumentBodyAttributeKey], 
									BinaryStorageOptions.GenericConstants.Constants[BinaryStorageOptions.CrmConstants.AttachmentEntityName][BinaryStorageOptions.GenericConstants.FileNameAttributeKey], external,
									moveAnnotations))
								{
									NotifyError(string.Format("Migration of '{0}' with id '{1}' FAILED.", BinaryStorageOptions.CrmConstants.AnnotationEntityName, id));
								}
								pgbProgress.InvokeEx(pb => pb.Increment(1));
								lblProgress.InvokeEx(pl => pl.Text = Math.Round(((double)pgbProgress.Value / (double)totalCount) * 100, 1) + "%");
								//Notify(string.Format("Waiting for {0} milliseconds...", waitTime));
								System.Threading.Thread.Sleep(waitTime);
							});
					}
					if (shouldCancel) break;
				}
			}
			finally
			{
				if (chkPluginStepsManage.Checked)
				{
					SetPluginStepsStatus(proxy, false);
				}
				isBusy = false;
				shouldCancel = false;
				btnMigrate.InvokeEx(b => b.Text = "Migrate");
				migrateGroup.InvokeEx(g => g.Controls.OfType<Control>().ToList().ForEach(c => c.Enabled = true));
				Notify("All Done.");
			}
		}

		private bool MigrateEntity(Guid id, string entityName, string documentBodyKey, string filenameKey, bool moveExternal, bool moveAnnotations)
		{
			OrganizationServiceProxy localProxy = null;
			bool success = false;
			try
			{
				if (IfdSelected)
				{
					localProxy = new OrganizationServiceProxy(serviceManagement, authCredentials.SecurityTokenResponse);
				}
				else
				{
					localProxy = new OrganizationServiceProxy(serviceManagement, authCredentials.ClientCredentials);
				}
				BinaryStorageOptions.Configuration.IConfigurationProvider configProvider = BinaryStorageOptions.Configuration.Factory.GetConfigurationProvider(proxy, entityName, GetUnsecurePluginConfiguration(localProxy, entityName), GetSecurePluginConfiguration());
				BinaryStorageOptions.Providers.IBinaryStorageProvider storageProvider = BinaryStorageOptions.Providers.Factory.GetStorageProvider(configProvider);
				Entity entity = localProxy.Retrieve(entityName, id, new ColumnSet(true));
				if (moveExternal)
				{
					success = MigrateEntityToExternal(localProxy, storageProvider, entity, entityName, documentBodyKey, filenameKey, moveAnnotations);
				}
				else
				{
					success = MigrateEntityToLocal(localProxy, storageProvider, entity, entityName, documentBodyKey, filenameKey);
				}
			}
			catch (Exception ex)
			{
				NotifyError(ex.ToString());
			}
			finally
			{
				if (localProxy != null)
				{
					localProxy.Dispose();
					localProxy = null;
				}
			}
			return success;
		}

		private bool MigrateEntityToExternal(OrganizationServiceProxy localProxy, BinaryStorageOptions.Providers.IBinaryStorageProvider storageProvider, Entity entity, string entityName, string documentBodyKey, string filenameKey, bool moveAnnotations)
		{
			if (!entity.Attributes.ContainsKey(documentBodyKey) || (string)entity.Attributes[documentBodyKey] == BinaryStorageOptions.GenericConstants.EmptyBodyContent)
				return true;

			string fileName = (string)entity.Attributes[filenameKey];
			Notify(string.Format("Migrating '{0}' with filename '{1}' CRM -> External using {2}.", entityName, entity.Id.ToString() + "_" + fileName, storageProvider.GetType().Name));
			if (storageProvider.Create(entity.Id, fileName, Convert.FromBase64String((string)entity.Attributes[documentBodyKey])))
			{
				Notify(string.Format("Created '{0}' with filename '{1}'", entityName, entity.Id.ToString() + "_" + fileName));
				if (entityName == BinaryStorageOptions.CrmConstants.AttachmentEntityName || moveAnnotations)
				{
					Notify(string.Format("Removing '{0}' with filename '{1}' from CRM.", entityName, entity.Id.ToString() + "_" + fileName));
					entity.Attributes[BinaryStorageOptions.GenericConstants.Constants[entity.LogicalName][BinaryStorageOptions.GenericConstants.DocumentBodyAttributeKey]] = BinaryStorageOptions.GenericConstants.EmptyBodyContent;
					localProxy.Update(entity);
				}
				Notify(string.Format("Migration of '{0}' with filename '{1}' DONE.", entityName, entity.Id.ToString() + "_" + fileName));
				return true;
			}
			return false;
		}

		private bool MigrateEntityToLocal(OrganizationServiceProxy localProxy, BinaryStorageOptions.Providers.IBinaryStorageProvider storageProvider, Entity entity, string entityName, string documentBodyKey, string filenameKey)
		{
			if (!entity.Attributes.ContainsKey(documentBodyKey))
				return true;

			string filename = (string)entity.Attributes[filenameKey];
			Notify(string.Format("Migrating '{0}' with filename '{1}' External -> CRM using {2}.", entityName, entity.Id.ToString() + "_" + filename, storageProvider.GetType().Name));
			if ((string)entity.Attributes[documentBodyKey] == BinaryStorageOptions.GenericConstants.EmptyBodyContent)
			{
				byte[] data = storageProvider.Read(entity.Id, filename);
				entity.Attributes[documentBodyKey] = Convert.ToBase64String(data);
				localProxy.Update(entity);
			}
			storageProvider.Delete(entity.Id, filename);
			Notify(string.Format("Migration of '{0}' with id '{1}' DONE.", entityName, entity.Id.ToString() + "_" + filename));
			return true;
		}

		private bool IfdSelected
		{
			get
			{
				bool value = false;
				if (rdoIFDAuth.InvokeRequired)
				{
					rdoIFDAuth.BeginInvoke(new MethodInvoker(delegate { value = rdoIFDAuth.Checked; }));
				}
				else
				{
					value = rdoIFDAuth.Checked;
				}
				return value;
			}
		}

		private void FillMigrationQueues(bool moveToExternal, bool migrateAnnotations, bool migrateEmailAttachments)
		{
			OrganizationServiceProxy localProxy = null;
			if (IfdSelected)
			{
				localProxy = new OrganizationServiceProxy(serviceManagement, authCredentials.SecurityTokenResponse);
			}
			else
			{
				localProxy = new OrganizationServiceProxy(serviceManagement, authCredentials.ClientCredentials);
			}

			annotationsToMigrate = new ConcurrentQueue<Guid>();
			attachmentsToMigrate = new ConcurrentQueue<Guid>();

			var annotationIdList = new List<Guid>();
			var attachmentsIdList = new List<Guid>();

			string pagingCookie = null;
			QueryExpression queryExpression;
			int pageSize = 5000;
			try
			{
				BinaryStorageOptions.Configuration.IConfigurationProvider configProvider;
				BinaryStorageOptions.Providers.IBinaryStorageProvider storageProvider;
				if (migrateAnnotations)
				{
					Notify(string.Format("Getting list of '{0}' to migrate...", BinaryStorageOptions.CrmConstants.AnnotationEntityName));
					configProvider = BinaryStorageOptions.Configuration.Factory.GetConfigurationProvider(proxy, BinaryStorageOptions.CrmConstants.AnnotationEntityName, GetUnsecurePluginConfiguration(proxy, BinaryStorageOptions.CrmConstants.AnnotationEntityName), GetSecurePluginConfiguration());
					storageProvider = BinaryStorageOptions.Providers.Factory.GetStorageProvider(configProvider);
					var externalAnnotations = new Dictionary<Guid, string>(storageProvider.GetFileNames().ToDictionary(fn => new Guid(fn.Substring(0, 36))));
					annotationIdList = new List<Guid>(externalAnnotations.Keys);
				}
				if (migrateEmailAttachments)
				{
					Notify(string.Format("Getting list of '{0}' to migrate...", BinaryStorageOptions.CrmConstants.AttachmentEntityName));
					configProvider = BinaryStorageOptions.Configuration.Factory.GetConfigurationProvider(proxy, BinaryStorageOptions.CrmConstants.AttachmentEntityName, GetUnsecurePluginConfiguration(proxy, BinaryStorageOptions.CrmConstants.AttachmentEntityName), GetSecurePluginConfiguration());
					storageProvider = BinaryStorageOptions.Providers.Factory.GetStorageProvider(configProvider);
					var externalAttachments = new Dictionary<Guid, string>(storageProvider.GetFileNames().ToDictionary(fn => new Guid(fn.Substring(0, 36))));
					attachmentsIdList = new List<Guid>(externalAttachments.Keys);
				}

				if (moveToExternal)
				{
					bool moreRecords = false;
					int pageNum = 1;
					EntityCollection results;
					if (migrateAnnotations)
					{
						Notify(string.Format("Getting list of '{0}' to migrate...", BinaryStorageOptions.CrmConstants.AnnotationEntityName));

						queryExpression = new QueryExpression(BinaryStorageOptions.CrmConstants.AnnotationEntityName);
						queryExpression.Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression();
						queryExpression.Criteria.AddCondition(BinaryStorageOptions.CrmConstants.IsDocumentKey, ConditionOperator.Equal, true);
						queryExpression.ColumnSet = new ColumnSet(false);
						queryExpression.PageInfo = new PagingInfo
						{
							Count = pageSize,
							PageNumber = pageNum,
							PagingCookie = pagingCookie,
						};
						results = localProxy.RetrieveMultiple(queryExpression);
						pagingCookie = results.PagingCookie;
						moreRecords = results.MoreRecords;
						while (moreRecords || results.Entities.Count > 0)
						{
							results.Entities.Select(e => e.Id).ToList().ForEach(id =>
							{
								if (!annotationIdList.Contains(id))
									annotationsToMigrate.Enqueue(id);
							});
							Notify(string.Format("{0} found : {1}", BinaryStorageOptions.CrmConstants.AnnotationEntityName, annotationsToMigrate.Count));
							queryExpression.PageInfo.PageNumber = ++pageNum;
							queryExpression.PageInfo.PagingCookie = pagingCookie;
							results = localProxy.RetrieveMultiple(queryExpression);
							pagingCookie = results.PagingCookie;
							moreRecords = results.MoreRecords;
						}
					}

					if (migrateEmailAttachments)
					{
						Notify(string.Format("Getting list of '{0}' to migrate...", BinaryStorageOptions.CrmConstants.AttachmentEntityName));

						moreRecords = false;
						pageNum = 1;
						pagingCookie = null;
						queryExpression = new QueryExpression(BinaryStorageOptions.CrmConstants.AttachmentEntityName);
						queryExpression.ColumnSet = new ColumnSet(false);
						queryExpression.PageInfo = new PagingInfo
						{
							Count = pageSize,
							PageNumber = pageNum,
							PagingCookie = pagingCookie,
						};
						results = localProxy.RetrieveMultiple(queryExpression);
						pagingCookie = results.PagingCookie;
						moreRecords = results.MoreRecords;
						while (moreRecords || results.Entities.Count > 0)
						{
							results.Entities.Select(e => e.Id).ToList().ForEach(id =>
							{
								if (!attachmentsIdList.Contains(id))
									attachmentsToMigrate.Enqueue(id);
							});
							Notify(string.Format("{0} found : {1}", BinaryStorageOptions.CrmConstants.AttachmentEntityName, attachmentsToMigrate.Count));
							queryExpression.PageInfo.PageNumber = ++pageNum;
							queryExpression.PageInfo.PagingCookie = pagingCookie;
							results = localProxy.RetrieveMultiple(queryExpression);
							pagingCookie = results.PagingCookie;
							moreRecords = results.MoreRecords;
						}
					}
				}
				else
				{
					attachmentsToMigrate = new ConcurrentQueue<Guid>(attachmentsIdList);
					annotationsToMigrate = new ConcurrentQueue<Guid>(annotationIdList);
				}
			}
			finally
			{
				if (localProxy != null)
				{
					localProxy.Dispose();
					localProxy = null;
				}
			}
		}

		private void Notify(string message)
		{
			lblMessage.InvokeEx(l => l.Text = message);
		}

		private void NotifyError(string message, bool newline = true)
		{
			txtOutput.InvokeEx(tb =>
			{
				tb.Suspend();
				tb.Text += message + (newline ? "\r\n" : "");
				tb.Select(tb.TextLength, 0);
				tb.ScrollToCaret();
				tb.Resume();
			});
		}

		public void SetPluginStepsStatus(IOrganizationService proxy, bool disable)
		{
			int pluginStateCode = disable ? 1 : 0;
			int pluginStatusCode = disable ? 2 : 1;
			using (OrganizationServiceContext context = new OrganizationServiceContext(proxy))
			{
				var steps = context.CreateQuery("sdkmessageprocessingstep")
										.Where(s => s.GetAttributeValue<string>("name").StartsWith("BinaryStorageOptions."))
										.ToList();
				foreach (var step in steps)
				{
					Notify(string.Format("{0} plugin step '{1}'...", (disable ? "Disabling" : "Enabling"), step.Attributes["name"]));
					var response = proxy.Execute(new Microsoft.Crm.Sdk.Messages.SetStateRequest
					{
						EntityMoniker = new EntityReference("sdkmessageprocessingstep", step.Id),
						State = new OptionSetValue(pluginStateCode),
						Status = new OptionSetValue(pluginStatusCode)
					});
				}
			}
			return;
		}

		private string GetUnsecurePluginConfiguration(IOrganizationService proxy, string entityName)
		{
			using (OrganizationServiceContext context = new OrganizationServiceContext(proxy))
			{
				string stepName = string.Format("BinaryStorageOptions.Plugin: Create of {0}", entityName);
				var step = context.CreateQuery("sdkmessageprocessingstep")
										.Where(s => s.GetAttributeValue<string>("name") == stepName)
										.FirstOrDefault();
				if (step != null)
				{
					return (string)step.Attributes["configuration"];
				}
			}
			return null;
		}

		private string GetSecurePluginConfiguration()
		{
			return string.Format("<Settings><Setting key=\"StorageAccount\" value=\"{0}\" /><Setting key=\"StorageKey\" value=\"{1}\" /></Settings>", txtAzureAccount.Text, txtAzureAccountKey.Text);
		}

		private bool moveAnnotationsWarningShown = false;
		private void chkMoveAnnotations_CheckedChanged(object sender, EventArgs e)
		{
			if (chkMoveAnnotations.Checked && !moveAnnotationsWarningShown && rdoOutbound.Checked)
			{
				if (MessageBox.Show("If you use this function, the filesizes on the annotation attachments will all be set to 3, which will cause queries against the filesize to behave oddly. Are you sure you want to do this?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
				{
					moveAnnotationsWarningShown = true;
					chkMoveAnnotations.Checked = false;
				}
			}
		}

		private void rdoInbound_CheckedChanged(object sender, EventArgs e)
		{
			chkMoveAnnotations.Enabled = !rdoInbound.Checked;
			if (!chkMoveAnnotations.Enabled)
				chkMoveAnnotations.Checked = false;
		}
	}
}

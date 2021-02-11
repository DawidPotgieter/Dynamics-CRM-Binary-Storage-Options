using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azure.Storage
{
	public class TableStorageHelper : StorageHelper
	{
		private const string tableStorageUriTemplate = "https://{0}.table.{1}/";
		private const string tableStorageSecondaryUriTemplate = "https://{0}-secondary.table.{1}/";

		/*
			{
				 "value":[
						{
							 "PartitionKey":"AA",
							 "RowKey":"A556269A-F190-433D-92C7-390F90E27F69",
							 "Timestamp":"2013-08-22T00:20:16.3134645Z",
							 "FileSize":"1234"
						}
				 ]
			}
		*/
		private Regex fileSizeJsonRegex = new Regex(@"""FileSize"":(?<FileSize>\d*)", RegexOptions.Compiled);
		private Regex queryJsonRegex = new Regex(@"""RowKey"":""(?<RowKey>[a-z0-9-]*)""", RegexOptions.Compiled);

		public TableStorageHelper(string account, string endpointSuffix, string key, bool isSasToken) :
			base(tableStorageUriTemplate, tableStorageSecondaryUriTemplate, account, endpointSuffix, key, isSasToken, true)
		{
		}

		public bool InsertValues(string tableName, Guid id, int fileSize)
		{
			string contentTemplate = "{{ \"FileSize\": {0}, \"PartitionKey\": \"{1}\", \"RowKey\": \"{2}\" }}";
			string content = string.Format(contentTemplate, fileSize, GetPartitionKey(id), id);

			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
			var response = ExecuteRestRequest(HtmlVerb.POST, tableName, httpContent);
			return response.IsSuccessStatusCode;
		}

		public bool DeleteEntry(string tableName, Guid id)
		{
			Dictionary<string, string> additionalHeaders = new Dictionary<string, string>
			{
				{ "If-Match", "*" },
			};
			var response = ExecuteRestRequest(HtmlVerb.DELETE, string.Format("{0}(PartitionKey='{1}', RowKey='{2}')", tableName, GetPartitionKey(id), id), null, additionalHeaders);
			return response.IsSuccessStatusCode;
		}

		public bool CreateTable(string tableName)
		{
			Microsoft.WindowsAzure.Storage.NameValidator.ValidateTableName(tableName);

			string content = string.Format("{{ \"TableName\": \"{0}\" }}", tableName);
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
			MD5 hasher = MD5.Create();
			string md5 = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(content)));
			httpContent.Headers.Add("Content-Length", content.Length.ToString());
			httpContent.Headers.Add("Content-MD5", md5);
			var response = ExecuteRestRequest(HtmlVerb.POST, "Tables", httpContent);
			return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict; //Conflict means already exists
		}

		public int GetFileSize(string tableName, Guid id)
		{
			Dictionary<string, string> additionalHeaders = new Dictionary<string, string>
			{
				{ "Accept", "application/json;odata=nometadata" },
			};
			var response = ExecuteRestRequest(HtmlVerb.GET, string.Format("{0}(PartitionKey='{1}', RowKey='{2}')?$select=FileSize", tableName, GetPartitionKey(id), id), null, additionalHeaders);
			if (response.IsSuccessStatusCode)
			{
				return int.Parse(fileSizeJsonRegex.Match(response.Content.ReadAsStringAsync().Result).Groups["FileSize"].Value);
			}
			return -1;
		}

		public List<Guid> Query(string tableName, string oDataFileSizeQuery)
		{
			List<Guid> ids = new List<Guid>();
			Dictionary<string, string> additionalHeaders = new Dictionary<string, string>
			{
				{ "Accept", "application/json;odata=nometadata" },
				{ "DataServiceVersion", "3.0;NetFx" },
				{ "MaxDataServiceVersion", "3.0;NetFx" },
			};
			var response = ExecuteRestRequest(HtmlVerb.GET, string.Format("{0}?$filter={1}&$select=RowKey", tableName, oDataFileSizeQuery), null, additionalHeaders);
			if (response.IsSuccessStatusCode)
			{
				string nextPartionKey = response.Headers.Contains("x-ms-continuation-NextPartitionKey") ? response.Headers.Single(h => h.Key == "x-ms-continuation-NextPartitionKey").Value.Single() : null;
				string nextRowKey = response.Headers.Contains("x-ms-continuation-NextRowKey") ? response.Headers.Single(h => h.Key == "x-ms-continuation-NextRowKey").Value.Single() : null;
				ids.AddRange(queryJsonRegex.Matches(response.Content.ReadAsStringAsync().Result).OfType<Match>().Select(m => m.Groups["RowKey"]).Select(c => Guid.Parse(c.Value)));
				while (nextPartionKey != null && response.IsSuccessStatusCode)
				{
					response = ExecuteRestRequest(HtmlVerb.GET, string.Format("{0}?NextPartitionKey={1}&{2}$filter={3}&$select=RowKey", tableName, nextPartionKey, (nextRowKey != null ? "NextRowKey=" + nextRowKey + "&" : ""), oDataFileSizeQuery), null, additionalHeaders);
					ids.AddRange(queryJsonRegex.Matches(response.Content.ReadAsStringAsync().Result).OfType<Match>().Select(m => m.Groups["RowKey"]).Select(c => Guid.Parse(c.Value)));
					nextPartionKey = response.Headers.Contains("x-ms-continuation-NextPartitionKey") ? response.Headers.Single(h => h.Key == "x-ms-continuation-NextPartitionKey").Value.Single() : null;
					nextRowKey = nextRowKey = response.Headers.Contains("x-ms-continuation-NextRowKey") ? response.Headers.Single(h => h.Key == "x-ms-continuation-NextRowKey").Value.Single() : null;
				}
			}
			return ids;
		}

		private string GetPartitionKey(Guid id)
		{
			//Use last two characters of id as partionkey.
			string idString = id.ToString();
			return idString.Substring(idString.Length - 1);
		}
	}
}

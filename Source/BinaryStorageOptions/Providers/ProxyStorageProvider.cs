using Azure.Storage;
using BinaryStorageOptions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace BinaryStorageOptions.Providers
{
	public class ProxyStorageProvider : IStorageProvider
	{
		ProxyStorageConfiguration configuration;
		private XNamespace serializationNamespace = "http://schemas.microsoft.com/2003/10/Serialization/";
		private XNamespace arraysNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
		private XNamespace proxyNamespace = "http://schemas.datacontract.org/2004/07/BinaryStorageOptions.Proxy.Models";

		public ProxyStorageProvider(ProxyStorageConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public List<string> GetFileNames()
		{
			List<string> fileNames = new List<string>();
			using (HttpClient httpClient = GetHttpClient())
			{
				HttpResponseMessage responseMessage = httpClient.GetAsync($"{configuration.Uri}/{configuration.Folder}/GetFileNames").Result;
				XDocument xDocument = XDocument.Load(responseMessage.Content.ReadAsStreamAsync().Result);
				foreach (var fileName in xDocument.Descendants(arraysNamespace + "string"))
				{
					fileNames.Add(fileName.Value);
				}
			}
			return fileNames;
		}

		public bool Create(Guid id, string filename, byte[] data, Dictionary<string, string> metaData = null)
		{
			using (HttpClient httpClient = GetHttpClient())
			{
				string metaDataQuery = null;
				if (metaData != null)
				{
					int index = 0;
					metaDataQuery = string.Join("&", metaData.Select(m => $"metaData[{index}].Key={m.Key}&metaData[{index++}].Value={m.Value}"));
				}
				HttpResponseMessage responseMessage = httpClient.PutAsync($"{configuration.Uri}/{configuration.Folder}/Create?id={id}&filename={filename}" + (metaData != null ? "&" + metaDataQuery : ""), new ByteArrayContent(data)).Result;
				XDocument xDocument = XDocument.Load(responseMessage.Content.ReadAsStreamAsync().Result);
				return bool.Parse(xDocument.Descendants(serializationNamespace + "boolean").First().Value);
			}
		}

		public bool Delete(Guid id, string filename)
		{
			using (HttpClient httpClient = GetHttpClient())
			{
				HttpResponseMessage responseMessage = httpClient.DeleteAsync($"{configuration.Uri}/{configuration.Folder}/Delete?id={id}&filename={filename}").Result;
				XDocument xDocument = XDocument.Load(responseMessage.Content.ReadAsStreamAsync().Result);
				return bool.Parse(xDocument.Descendants(serializationNamespace + "boolean").First().Value);
			}
		}

		public byte[] Read(Guid id, string filename, out Dictionary<string, string> metaData)
		{
			metaData = new Dictionary<string, string>();
			byte[] data = null;
			using (HttpClient httpClient = GetHttpClient())
			{
				HttpResponseMessage responseMessage = httpClient.GetAsync($"{configuration.Uri}/{configuration.Folder}/Read?id={id}&filename={filename}").Result;
				XDocument xDocument = XDocument.Load(responseMessage.Content.ReadAsStreamAsync().Result);
				data = Convert.FromBase64String(xDocument.Descendants(proxyNamespace + "Data").First().Value);
				foreach (var metaDataItem in xDocument.Descendants(proxyNamespace + "MetaData").First().Elements())
				{
					metaData.Add(metaDataItem.Elements().Skip(0).Take(1).Single().Value, metaDataItem.Elements().Skip(1).Take(1).Single().Value);
				}
			}

			return data;
		}

		public Dictionary<string, string> GetMetaData(Guid id, string filename, bool customOnly = true)
		{
			Dictionary<string, string> metaData = new Dictionary<string, string>();
			using (HttpClient httpClient = GetHttpClient())
			{
				HttpResponseMessage responseMessage = httpClient.GetAsync($"{configuration.Uri}/{configuration.Folder}/GetMetaData?id={id}&filename={filename}").Result;
				XDocument xDocument = XDocument.Load(responseMessage.Content.ReadAsStreamAsync().Result);
				foreach (var metaDataItem in xDocument.Descendants(arraysNamespace + "KeyValueOfstringstring"))
				{
					metaData.Add(metaDataItem.Elements().Skip(0).Take(1).Single().Value, metaDataItem.Elements().Skip(1).Take(1).Single().Value);
				}
			}
			return metaData;
		}

		public int GetFileSize(Guid id, string filename)
		{
			using (HttpClient httpClient = GetHttpClient())
			{
				HttpResponseMessage responseMessage = httpClient.GetAsync($"{configuration.Uri}/{configuration.Folder}/GetFileSize?id={id}&filename={filename}").Result;
				XDocument xDocument = XDocument.Load(responseMessage.Content.ReadAsStreamAsync().Result);
				return int.Parse(xDocument.Descendants(serializationNamespace + "int").First().Value);
			}
		}

		private HttpClient GetHttpClient()
		{
			HttpClient client;
			if (string.IsNullOrWhiteSpace(configuration.Username) || string.IsNullOrWhiteSpace(configuration.Password))
			{
				client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
			}
			else
			{
				client = new HttpClient();
				var basicAuthBytes = Encoding.ASCII.GetBytes($"{configuration.Username}:{configuration.Password}");
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(basicAuthBytes));
			}
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));
			return client;
		}
	}
}

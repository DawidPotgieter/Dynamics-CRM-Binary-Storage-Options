﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BinaryStorageOptions.Proxy.BasicAuthenticationModule
{
	public class AuthenticationFailureResult : IHttpActionResult
	{
		public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
		{
			ReasonPhrase = reasonPhrase;
			Request = request;
		}

		public string ReasonPhrase { get; private set; }

		public HttpRequestMessage Request { get; private set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute());
		}

		private HttpResponseMessage Execute()
		{
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			response.RequestMessage = Request;
			response.ReasonPhrase = ReasonPhrase;
			return response;
		}
	}
}
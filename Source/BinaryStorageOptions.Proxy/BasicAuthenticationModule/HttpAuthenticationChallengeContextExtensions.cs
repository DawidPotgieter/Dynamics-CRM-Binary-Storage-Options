﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Filters;

namespace BinaryStorageOptions.Proxy.BasicAuthenticationModule
{
	public static class HttpAuthenticationChallengeContextExtensions
	{
		public static void ChallengeWith(this HttpAuthenticationChallengeContext context, string scheme)
		{
			ChallengeWith(context, new AuthenticationHeaderValue(scheme));
		}

		public static void ChallengeWith(this HttpAuthenticationChallengeContext context, string scheme, string parameter)
		{
			ChallengeWith(context, new AuthenticationHeaderValue(scheme, parameter));
		}

		public static void ChallengeWith(this HttpAuthenticationChallengeContext context, AuthenticationHeaderValue challenge)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
		}
	}
}
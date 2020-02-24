using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerTools.WebAPI
{
	// Token: 0x02000015 RID: 21
	public static class OpenID
	{
		// Token: 0x06000069 RID: 105 RVA: 0x00007760 File Offset: 0x00005960
		static OpenID()
		{
			for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
			{
				if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-debugopenid"))
				{
					OpenID.debugOpenId = true;
				}
			}
			ServicePointManager.ServerCertificateValidationCallback = delegate (object _srvPoint, X509Certificate _certificate, X509Chain _chain, SslPolicyErrors _errors)
			{
				if (_errors == SslPolicyErrors.None)
				{
					return true;
				}
				X509Chain x509Chain = new X509Chain();
				x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
				x509Chain.ChainPolicy.ExtraStore.Add(OpenID.caCert);
				x509Chain.ChainPolicy.ExtraStore.Add(OpenID.caIntermediateCert);
				if (x509Chain.Build(new X509Certificate2(_certificate)))
				{
					x509Chain.Reset();
					return true;
				}
				if (x509Chain.ChainStatus.Length == 0)
				{
					x509Chain.Reset();
					return true;
				}
				foreach (X509ChainElement x509ChainElement in x509Chain.ChainElements)
				{
					foreach (X509ChainStatus x509ChainStatus in x509ChainElement.ChainElementStatus)
					{
						if (x509ChainStatus.Status != X509ChainStatusFlags.NoError && (x509ChainStatus.Status != X509ChainStatusFlags.UntrustedRoot || !x509ChainElement.Certificate.Equals(OpenID.caCert)))
						{
							Log.Warning(string.Concat(new object[]
							{
								"Steam certificate error: ",
								x509ChainElement.Certificate.Subject,
								" ### Error: ",
								x509ChainStatus.Status
							}));
							x509Chain.Reset();
							return false;
						}
					}
				}
				foreach (X509ChainStatus x509ChainStatus2 in x509Chain.ChainStatus)
				{
					if (x509ChainStatus2.Status != X509ChainStatusFlags.NoError && x509ChainStatus2.Status != X509ChainStatusFlags.UntrustedRoot)
					{
						Log.Warning("Steam certificate error: " + x509ChainStatus2.Status);
						x509Chain.Reset();
						return false;
					}
				}
				x509Chain.Reset();
				return true;
			};
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00007800 File Offset: 0x00005A00
		public static string GetOpenIdLoginUrl(string _returnHost, string _returnUrl)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("openid.ns", "http://specs.openid.net/auth/2.0");
			dictionary.Add("openid.mode", "checkid_setup");
			dictionary.Add("openid.return_to", _returnUrl);
			dictionary.Add("openid.realm", _returnHost);
			dictionary.Add("openid.identity", "http://specs.openid.net/auth/2.0/identifier_select");
			dictionary.Add("openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select");
			return "https://steamcommunity.com/openid/login" + '?' + OpenID.buildUrlParams(dictionary);
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00007884 File Offset: 0x00005A84
		public static ulong Validate(HttpListenerRequest _req)
		{
			string value = OpenID.getValue(_req, "openid.mode");
			if (value == "cancel")
			{
				Log.Warning("Steam OpenID login canceled");
				return 0UL;
			}
			if (value == "error")
			{
				Log.Warning("Steam OpenID login error: " + OpenID.getValue(_req, "openid.error"));
				if (OpenID.debugOpenId)
				{
					OpenID.PrintOpenIdResponse(_req);
				}
				return 0UL;
			}
			string value2 = OpenID.getValue(_req, "openid.claimed_id");
			Match match = OpenID.steamIdUrlMatcher.Match(value2);
			if (!match.Success)
			{
				Log.Warning("Steam OpenID login result did not give a valid SteamID");
				if (OpenID.debugOpenId)
				{
					OpenID.PrintOpenIdResponse(_req);
				}
				return 0UL;
			}
			ulong result = ulong.Parse(match.Groups[1].Value);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("openid.ns", "http://specs.openid.net/auth/2.0");
			dictionary.Add("openid.assoc_handle", OpenID.getValue(_req, "openid.assoc_handle"));
			dictionary.Add("openid.signed", OpenID.getValue(_req, "openid.signed"));
			dictionary.Add("openid.sig", OpenID.getValue(_req, "openid.sig"));
			dictionary.Add("openid.identity", "http://specs.openid.net/auth/2.0/identifier_select");
			dictionary.Add("openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select");
			string[] array = OpenID.getValue(_req, "openid.signed").Split(new char[]
			{
				','
			});
			foreach (string str in array)
			{
				dictionary["openid." + str] = OpenID.getValue(_req, "openid." + str);
			}
			dictionary.Add("openid.mode", "check_authentication");
			byte[] bytes = Encoding.ASCII.GetBytes(OpenID.buildUrlParams(dictionary));
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/openid/login");
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			httpWebRequest.ContentLength = (long)bytes.Length;
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en");
			using (Stream requestStream = httpWebRequest.GetRequestStream())
			{
				requestStream.Write(bytes, 0, bytes.Length);
			}
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			string text;
			using (Stream responseStream = httpWebResponse.GetResponseStream())
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					text = streamReader.ReadToEnd();
				}
			}
			if (text.ContainsCaseInsensitive("is_valid:true"))
			{
				return result;
			}
			Log.Warning("Steam OpenID login failed: {0}", new object[]
			{
				text
			});
			return 0UL;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00007B64 File Offset: 0x00005D64
		private static string buildUrlParams(Dictionary<string, string> _queryParams)
		{
			string[] array = new string[_queryParams.Count];
			int num = 0;
			foreach (KeyValuePair<string, string> keyValuePair in _queryParams)
			{
				array[num++] = keyValuePair.Key + "=" + Uri.EscapeDataString(keyValuePair.Value);
			}
			return string.Join("&", array);
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00007BEC File Offset: 0x00005DEC
		private static string getValue(HttpListenerRequest _req, string _name)
		{
			NameValueCollection queryString = _req.QueryString;
			if (queryString[_name] == null)
			{
				throw new MissingMemberException("OpenID parameter \"" + _name + "\" missing");
			}
			return queryString[_name];
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00007C28 File Offset: 0x00005E28
		private static void PrintOpenIdResponse(HttpListenerRequest _req)
		{
			NameValueCollection queryString = _req.QueryString;
			for (int i = 0; i < queryString.Count; i++)
			{
				Log.Out("   " + queryString.GetKey(i) + " = " + queryString[i]);
			}
		}

		// Token: 0x04000031 RID: 49
		private const string STEAM_LOGIN = "https://steamcommunity.com/openid/login";

		// Token: 0x04000032 RID: 50
		private static readonly Regex steamIdUrlMatcher = new Regex("^https?:\\/\\/steamcommunity\\.com\\/openid\\/id\\/([0-9]{17,18})");

		// Token: 0x04000033 RID: 51
		private static readonly X509Certificate2 caCert = new X509Certificate2(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/steam-rootca.cer");

		// Token: 0x04000034 RID: 52
		private static readonly X509Certificate2 caIntermediateCert = new X509Certificate2(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/steam-intermediate.cer");

		// Token: 0x04000035 RID: 53
		private const bool verboseSsl = false;

		// Token: 0x04000036 RID: 54
		public static bool debugOpenId;
	}
}
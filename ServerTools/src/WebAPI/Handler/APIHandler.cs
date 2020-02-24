using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace ServerTools.WebAPI
{
	// Token: 0x0200001A RID: 26
	public class APIHandler : PathHandler
	{
		// Token: 0x0600008C RID: 140 RVA: 0x000089B0 File Offset: 0x00006BB0
		public APIHandler(string _staticPart, string _moduleName = null) : base(_moduleName, 0)
		{
			this.staticPart = _staticPart;
			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(WebAPI)))
				{
					ConstructorInfo constructor = type.GetConstructor(new Type[0]);
					if (constructor != null)
					{
						WebAPI webAPI = (WebAPI)constructor.Invoke(new object[0]);
						this.AddApi(webAPI.Name, webAPI);
					}
				}
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x000023B9 File Offset: 0x000005B9
		private void AddApi(string _apiName, WebAPI _api)
		{
			this.apis.Add(_apiName, _api);
			WebPermissions.Instance.AddKnownModule("webapi." + _apiName, _api.DefaultPermissionLevel());
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00008A9C File Offset: 0x00006C9C
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			string text = _req.Url.AbsolutePath.Remove(0, this.staticPart.Length);
			WebAPI webAPI;
			if (!this.apis.TryGetValue(text, out webAPI))
			{
				Log.Out("Error in ApiHandler.HandleRequest(): No handler found for API \"" + text + "\"");
				_resp.StatusCode = 404;
				return;
			}
			if (!this.AuthorizeForCommand(text, _user, _permissionLevel))
			{
				_resp.StatusCode = 403;
				if (_user == null)
				{
				}
				return;
			}
			try
			{
				webAPI.HandleRequest(_req, _resp, _user, _permissionLevel);
			}
			catch (Exception ex)
			{
				Log.Error("Error in ApiHandler.HandleRequest(): Handler {0} threw an exception:", new object[]
				{
					webAPI.Name
				});
				Log.Exception(ex);
				_resp.StatusCode = 500;
			}
		}

		// Token: 0x0600008F RID: 143 RVA: 0x000023E3 File Offset: 0x000005E3
		private bool AuthorizeForCommand(string _apiName, WebConnect _user, int _permissionLevel)
		{
			return WebPermissions.Instance.ModuleAllowedWithLevel("webapi." + _apiName, _permissionLevel);
		}

		// Token: 0x04000046 RID: 70
		private readonly Dictionary<string, WebAPI> apis = new CaseInsensitiveStringDictionary<WebAPI>();

		// Token: 0x04000047 RID: 71
		private readonly string staticPart;
	}
}
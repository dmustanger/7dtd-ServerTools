using System.Net;
using ServerTools.WebAPI.JSON;

namespace ServerTools.WebAPI
{
	// Token: 0x02000022 RID: 34
	public class UserStatusHandler : PathHandler
	{
		// Token: 0x060000A9 RID: 169 RVA: 0x000024B1 File Offset: 0x000006B1
		public UserStatusHandler(string _moduleName = null) : base(_moduleName, 0)
		{
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00009464 File Offset: 0x00007664
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			JSONObject jsonobject = new JSONObject();
			jsonobject.Add("loggedin", new JSONBoolean(_user != null));
			jsonobject.Add("username", new JSONString((_user == null) ? string.Empty : _user.SteamID.ToString()));
			JSONArray jsonarray = new JSONArray();
			foreach (WebPermissions.WebModulePermission webModulePermission in WebPermissions.Instance.GetModules())
			{
				JSONObject jsonobject2 = new JSONObject();
				jsonobject2.Add("module", new JSONString(webModulePermission.module));
				jsonobject2.Add("allowed", new JSONBoolean(webModulePermission.permissionLevel >= _permissionLevel));
				jsonarray.Add(jsonobject2);
			}
			jsonobject.Add("permissions", jsonarray);
			WebAPI.WriteJSON(_resp, jsonobject);
		}
	}
}

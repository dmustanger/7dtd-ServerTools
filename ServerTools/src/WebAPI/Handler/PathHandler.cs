using System.Net;

namespace ServerTools.WebAPI
{
	// Token: 0x0200001C RID: 28
	public abstract class PathHandler
	{
		// Token: 0x06000098 RID: 152 RVA: 0x0000243B File Offset: 0x0000063B
		protected PathHandler(string _moduleName, int _defaultPermissionLevel = 0)
		{
			this.moduleName = _moduleName;
			WebPermissions.Instance.AddKnownModule(_moduleName, _defaultPermissionLevel);
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000099 RID: 153 RVA: 0x00002456 File Offset: 0x00000656
		public string ModuleName
		{
			get
			{
				return this.moduleName;
			}
		}

		// Token: 0x0600009A RID: 154
		public abstract void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel);

		// Token: 0x0600009B RID: 155 RVA: 0x0000245E File Offset: 0x0000065E
		public bool IsAuthorizedForHandler(WebConnect _user, int _permissionLevel)
		{
			return this.moduleName == null || WebPermissions.Instance.ModuleAllowedWithLevel(this.moduleName, _permissionLevel);
		}

		// Token: 0x0400004D RID: 77
		private readonly string moduleName;
	}
}
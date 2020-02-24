using System.Net;

namespace ServerTools.WebAPI
{
	// Token: 0x0200001D RID: 29
	public class SimpleRedirectHandler : PathHandler
	{
		// Token: 0x0600009C RID: 156 RVA: 0x0000247B File Offset: 0x0000067B
		public SimpleRedirectHandler(string _target, string _moduleName = null) : base(_moduleName, 0)
		{
			this.target = _target;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0000248C File Offset: 0x0000068C
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			_resp.Redirect(this.target);
		}

		// Token: 0x0400004E RID: 78
		private readonly string target;
	}
}
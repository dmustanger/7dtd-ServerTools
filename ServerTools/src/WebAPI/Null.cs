using System.Net;
using System.Text;

namespace ServerTools.WebAPI
{
	// Token: 0x0200000A RID: 10
	public class Null : WebAPI
	{
		// Token: 0x06000034 RID: 52 RVA: 0x000021E5 File Offset: 0x000003E5
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			_resp.ContentLength64 = 0L;
			_resp.ContentType = "text/plain";
			_resp.ContentEncoding = Encoding.ASCII;
			_resp.OutputStream.Write(new byte[0], 0, 0);
		}
	}
}
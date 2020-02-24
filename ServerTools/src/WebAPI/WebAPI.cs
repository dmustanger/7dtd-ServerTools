using ServerTools.WebAPI.JSON;
using System.Net;
using System.Text;

namespace ServerTools.WebAPI
{
	// Token: 0x0200000E RID: 14
	public abstract class WebAPI
	{
		// Token: 0x06000043 RID: 67 RVA: 0x0000222F File Offset: 0x0000042F
		protected WebAPI()
		{
			this.Name = base.GetType().Name;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00006B48 File Offset: 0x00004D48
		public static void WriteJSON(HttpListenerResponse _resp, JSONNode _root)
		{
			StringBuilder stringBuilder = new StringBuilder();
			_root.ToString(stringBuilder, false, 0);
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			_resp.ContentLength64 = (long)bytes.Length;
			_resp.ContentType = "application/json";
			_resp.ContentEncoding = Encoding.UTF8;
			_resp.OutputStream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00006BA8 File Offset: 0x00004DA8
		public static void WriteText(HttpListenerResponse _resp, string _text)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(_text);
			_resp.ContentLength64 = (long)bytes.Length;
			_resp.ContentType = "text/plain";
			_resp.ContentEncoding = Encoding.UTF8;
			_resp.OutputStream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x06000046 RID: 70
		public abstract void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel);

		// Token: 0x06000047 RID: 71 RVA: 0x00002248 File Offset: 0x00000448
		public virtual int DefaultPermissionLevel()
		{
			return 0;
		}

		// Token: 0x0400002A RID: 42
		public readonly string Name;
	}
}

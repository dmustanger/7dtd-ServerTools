using System.IO;
using System.Net;
using System.Text;

namespace ServerTools.WebAPI
{
	// Token: 0x0200001F RID: 31
	public class SessionHandler : PathHandler
	{
		// Token: 0x060000A0 RID: 160 RVA: 0x000090E8 File Offset: 0x000072E8
		public SessionHandler(string _staticPart, string _dataFolder, Web _parent, string _moduleName = null) : base(_moduleName, 0)
		{
			this.staticPart = _staticPart;
			this.parent = _parent;
			if (File.Exists(_dataFolder + "/sessionheader.tmpl"))
			{
				this.header = File.ReadAllText(_dataFolder + "/sessionheader.tmpl");
			}
			if (File.Exists(_dataFolder + "/sessionfooter.tmpl"))
			{
				this.footer = File.ReadAllText(_dataFolder + "/sessionfooter.tmpl");
			}
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00009174 File Offset: 0x00007374
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			string text = _req.Url.AbsolutePath.Remove(0, this.staticPart.Length);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.header);
			if (text.StartsWith("verify"))
			{
				if (_user != null)
				{
					_resp.Redirect("/static/index.html");
					return;
				}
				stringBuilder.Append("<h1>Login failed, <a href=\"/static/index.html\">click to return to main page</a>.</h1>");
			}
			else if (text.StartsWith("logout"))
			{
				if (_user != null)
				{
					this.parent.connectionHandler.LogOut(_user.SessionID);
					_resp.AppendCookie(new Cookie("sid", string.Empty, "/")
					{
						Expired = true
					});
					_resp.Redirect("/static/index.html");
					return;
				}
				stringBuilder.Append("<h1>Not logged in, <a href=\"/static/index.html\">click to return to main page</a>.</h1>");
			}
			else
			{
				if (text.StartsWith("login"))
				{
					string text2 = ((!Web.IsSslRedirected(_req)) ? "http://" : "https://") + _req.UserHostName;
					string openIdLoginUrl = OpenID.GetOpenIdLoginUrl(text2, text2 + "/session/verify");
					_resp.Redirect(openIdLoginUrl);
					return;
				}
				stringBuilder.Append("<h1>Unknown command, <a href=\"/static/index.html\">click to return to main page</a>.</h1>");
			}
			stringBuilder.Append(this.footer);
			_resp.ContentType = MimeType.GetMimeType(".html");
			_resp.ContentEncoding = Encoding.UTF8;
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			_resp.ContentLength64 = (long)bytes.Length;
			_resp.OutputStream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x04000053 RID: 83
		private readonly string footer = string.Empty;

		// Token: 0x04000054 RID: 84
		private readonly string header = string.Empty;

		// Token: 0x04000055 RID: 85
		private readonly Web parent;

		// Token: 0x04000056 RID: 86
		private readonly string staticPart;
	}
}

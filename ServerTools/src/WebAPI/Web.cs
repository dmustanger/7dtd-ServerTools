using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using ServerTools.WebAPI.Cache;

namespace ServerTools.WebAPI
{
	// Token: 0x0200000B RID: 11
	public class Web : IConsoleServer
	{
		// Token: 0x06000035 RID: 53 RVA: 0x00003CB0 File Offset: 0x00001EB0
		public Web()
		{
			try
			{
				int @int = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("ControlPanelPort", false));//request current control panel port
				if (@int >= 1 && @int <= 65533)
				{
					if (!Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Website"))//does operating mod directory contain webserver folder
					{
						Log.Out("Webserver not started (folder \"Website\" not found in WebInterface mod folder)");
					}
					else
					{
						this.useStaticCache = false;
						this.dataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Website";//set folder containing website data
						if (!HttpListener.IsSupported)//is an http listener supported by this OS?
						{
							Log.Out("Webserver not started (needs Windows XP SP2, Server 2003 or later or Mono)");
						}
						else
						{
							this.handlers.Add("/index.htm", new SimpleRedirectHandler("/static/index.html", null));//add handler for default main page redirect
							//this.handlers.Add("/favicon.ico", new SimpleRedirectHandler("/static/favicon.ico", null));

							bool userLoggedin = true;

							if (userLoggedin)

							{
								Log.Out("User logged in...");
								this.handlers.Add("/session/", new SessionHandler("/session/", this.dataFolder, this, null));//add handler for user sessions
								this.handlers.Add("/userstatus", new UserStatusHandler(null));
								if (this.useStaticCache)
								{
									this.handlers.Add("/static/", new StaticHandler("/static/", this.dataFolder, new SimpleCache(), false, null));
								}
								else
								{
									this.handlers.Add("/static/", new StaticHandler("/static/", this.dataFolder, new DirectAccess(), false, null));
								}
								//this.handlers.Add("/itemicons/", new ItemIconHandler("/itemicons/", true, null));
								//this.handlers.Add("/map/", new StaticHandler("/map/", GameUtils.GetSaveGameDir(null, null) + "/map", MapRendering.GetTileCache(), false, "web.map"));
							}
							else
							{
								Log.Out("User Not logged in!");
							}
							this.handlers.Add("/api/", new APIHandler("/api/", null));
							this.connectionHandler = new Connection();
							this._listener.Prefixes.Add(string.Format("http://*:{0}/", @int + 2));
							this._listener.Start();
							SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterServer(this);
							this._listener.BeginGetContext(new AsyncCallback(this.HandleRequest), this._listener);
							Log.Out("[SERVERTOOLS] Started Webserver on " + (@int + 2));
						}
					}
				}
				else
				{
					Log.Out("Webserver not started (ControlPanelPort not within 1-65533)");
				}
			}
			catch (Exception e)
			{
				Log.Out(string.Format("[SERVERTOOLS] Error in Web: {0}", e.Message));
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003F5C File Offset: 0x0000215C
		public void Disconnect()
		{
			try
			{
				this._listener.Stop();
				this._listener.Close();
			}
			catch (Exception arg)
			{
				Log.Out("Error in Web.Disconnect: " + arg);
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x0000221F File Offset: 0x0000041F
		public void SendLine(string _line)
		{
			this.connectionHandler.SendLine(_line);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x0000222D File Offset: 0x0000042D
		public void SendLog(string _text, string _trace, LogType _type)
		{
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00003FA4 File Offset: 0x000021A4
		public static bool IsSslRedirected(HttpListenerRequest _req)
		{
			string text = _req.Headers["X-Forwarded-Proto"];
			return !string.IsNullOrEmpty(text) && text.Equals("https", StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003FD8 File Offset: 0x000021D8
		private void HandleRequest(IAsyncResult _result)
		{
			if (!this._listener.IsListening)
			{
				return;
			}
			Interlocked.Increment(ref Web.handlingCount);
			Interlocked.Increment(ref Web.currentHandlers);
			HttpListenerContext httpListenerContext = this._listener.EndGetContext(_result);
			this._listener.BeginGetContext(new AsyncCallback(this.HandleRequest), this._listener);
			try
			{
				HttpListenerRequest request = httpListenerContext.Request;
				HttpListenerResponse response = httpListenerContext.Response;
				response.SendChunked = false;
				response.ProtocolVersion = this.HttpProtocolVersion;
				int permissionLevel = this.DoAuthentication(request, out WebConnect webConnection);
				if (webConnection != null)
				{
					response.AppendCookie(new Cookie("sid", webConnection.SessionID, "/")
					{
						Expired = false,
						Expires = new DateTime(2021, 1, 1),
						HttpOnly = true,
						Secure = false
					});
				}
				if (GameManager.Instance.World == null)
				{
					response.StatusCode = 503;
				}
				else if (request.Url.AbsolutePath.Length < 2)
				{
					this.handlers["/index.htm"].HandleRequest(request, response, webConnection, permissionLevel);
				}
				else
				{
					foreach (KeyValuePair<string, PathHandler> keyValuePair in this.handlers)
					{
						if (request.Url.AbsolutePath.StartsWith(keyValuePair.Key))
						{
							if (!keyValuePair.Value.IsAuthorizedForHandler(webConnection, permissionLevel))
							{
								response.StatusCode = 403;
								if (webConnection != null)
								{
								}
							}
							else
							{
								keyValuePair.Value.HandleRequest(request, response, webConnection, permissionLevel);
							}
							return;
						}
					}
					response.StatusCode = 404;
				}
			}
			catch (IOException ex)
			{
				if (ex.InnerException is SocketException)
				{
					Log.Out("Error in Web.HandleRequest(): Remote host closed connection: " + ex.InnerException.Message);
				}
				else
				{
					Log.Out("Error (IO) in Web.HandleRequest(): " + ex);
				}
			}
			catch (Exception arg)
			{
				Log.Out("Error in Web.HandleRequest(): " + arg);
			}
			finally
			{
				if (httpListenerContext != null && !httpListenerContext.Response.SendChunked)
				{
					httpListenerContext.Response.Close();
				}
				Interlocked.Decrement(ref Web.currentHandlers);
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000426C File Offset: 0x0000246C
		private int DoAuthentication(HttpListenerRequest _req, out WebConnect _con)
		{
			_con = null;
			string text = null;
			if (_req.Cookies["sid"] != null)
			{
				text = _req.Cookies["sid"].Value;
			}
			if (!string.IsNullOrEmpty(text))
			{
				WebConnect webConnection = this.connectionHandler.IsLoggedIn(text, _req.RemoteEndPoint.Address);
				if (webConnection != null)
				{
					_con = webConnection;
					return GameManager.Instance.adminTools.GetAdminToolsClientInfo(_con.SteamID.ToString()).PermissionLevel;
				}
			}
			if (_req.QueryString["adminuser"] != null && _req.QueryString["admintoken"] != null)
			{
				WebPermissions.AdminToken webAdmin = WebPermissions.Instance.GetWebAdmin(_req.QueryString["adminuser"], _req.QueryString["admintoken"]);
				if (webAdmin != null)
				{
					return webAdmin.permissionLevel;
				}
				Log.Warning("Invalid Admintoken used from " + _req.RemoteEndPoint);
			}
			if (_req.Url.AbsolutePath.StartsWith("/session/verify", StringComparison.OrdinalIgnoreCase))
			{
				int result;
				try
				{
					ulong num = OpenID.Validate(_req);
					if (num <= 0UL)
					{
						Log.Out("Steam OpenID login failed from {0}", new object[]
						{
							_req.RemoteEndPoint.ToString()
						});
						return 2000;
					}
					WebConnect webConnection2 = this.connectionHandler.LogIn(num, _req.RemoteEndPoint.Address);
					_con = webConnection2;
					int permissionLevel = GameManager.Instance.adminTools.GetAdminToolsClientInfo(num.ToString()).PermissionLevel;
					Log.Out("Steam OpenID login from {0} with ID {1}, permission level {2}", new object[]
					{
						_req.RemoteEndPoint.ToString(),
						webConnection2.SteamID,
						permissionLevel
					});
					result = permissionLevel;
				}
				catch (Exception ex)
				{
					Log.Error("Error validating login:");
					Log.Exception(ex);
					return 2000;
				}
				return result;
			}
			return 2000;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x0000446C File Offset: 0x0000266C
		public static void SetResponseTextContent(HttpListenerResponse _resp, string _text)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(_text);
			_resp.ContentLength64 = (long)bytes.Length;
			_resp.ContentType = "text/html";
			_resp.ContentEncoding = Encoding.UTF8;
			_resp.OutputStream.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x04000020 RID: 32
		public static int handlingCount;

		// Token: 0x04000021 RID: 33
		public static int currentHandlers;

		// Token: 0x04000022 RID: 34
		public static long totalHandlingTime;

		// Token: 0x04000023 RID: 35
		private readonly HttpListener _listener = new HttpListener();

		// Token: 0x04000024 RID: 36
		private readonly string dataFolder;

		// Token: 0x04000025 RID: 37
		private readonly Dictionary<string, PathHandler> handlers = new CaseInsensitiveStringDictionary<PathHandler>();

		// Token: 0x04000026 RID: 38
		private readonly bool useStaticCache;

		// Token: 0x04000027 RID: 39
		public Connection connectionHandler;

		// Token: 0x04000028 RID: 40
		private readonly Version HttpProtocolVersion = new Version(1, 1);
	}
}
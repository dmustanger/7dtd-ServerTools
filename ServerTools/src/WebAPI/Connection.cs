using System;
using System.Collections.Generic;
using System.Net;

namespace ServerTools.WebAPI
{
	// Token: 0x02000016 RID: 22
	public class Connection
	{
		// Token: 0x06000071 RID: 113 RVA: 0x00007E0C File Offset: 0x0000600C
		public WebConnect IsLoggedIn(string _sessionId, IPAddress _ip)
		{
			if (!this.connections.ContainsKey(_sessionId))
			{
				return null;
			}
			WebConnect webConnection = this.connections[_sessionId];
			if (!object.Equals(webConnection.Endpoint, _ip))
			{
				return null;
			}
			webConnection.UpdateUsage();
			return webConnection;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000022FA File Offset: 0x000004FA
		public void LogOut(string _sessionId)
		{
			this.connections.Remove(_sessionId);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00007E50 File Offset: 0x00006050
		public WebConnect LogIn(ulong _steamId, IPAddress _ip)
		{
			string text = Guid.NewGuid().ToString();
			WebConnect webConnection = new WebConnect(text, _ip, _steamId);
			this.connections.Add(text, webConnection);
			return webConnection;
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00007E88 File Offset: 0x00006088
		public void SendLine(string _line)
		{
			foreach (KeyValuePair<string, WebConnect> keyValuePair in this.connections)
			{
				keyValuePair.Value.SendLine(_line);
			}
		}

		// Token: 0x04000037 RID: 55
		private readonly Dictionary<string, WebConnect> connections = new Dictionary<string, WebConnect>();
	}
}

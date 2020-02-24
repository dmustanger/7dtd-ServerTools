using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace ServerTools.WebAPI
{
	// Token: 0x02000014 RID: 20
	public class WebConnect : ConsoleConnectionAbstract
	{
		// Token: 0x0600005A RID: 90 RVA: 0x00007708 File Offset: 0x00005908
		public WebConnect(string _sessionId, IPAddress _endpoint, ulong _steamId)
		{
			this.SessionID = _sessionId;
			this.Endpoint = _endpoint;
			this.SteamID = _steamId;
			this.login = DateTime.Now;
			this.lastAction = this.login;
			this.conDescription = "WebPanel from " + this.Endpoint;
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00002269 File Offset: 0x00000469
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00002271 File Offset: 0x00000471
		public string SessionID { get; private set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600005D RID: 93 RVA: 0x0000227A File Offset: 0x0000047A
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00002282 File Offset: 0x00000482
		public IPAddress Endpoint { get; private set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600005F RID: 95 RVA: 0x0000228B File Offset: 0x0000048B
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00002293 File Offset: 0x00000493
		public ulong SteamID { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000061 RID: 97 RVA: 0x0000229C File Offset: 0x0000049C
		public TimeSpan Age
		{
			get
			{
				return DateTime.Now - this.lastAction;
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000022D2 File Offset: 0x000004D2
		public void UpdateUsage()
		{
			this.lastAction = DateTime.Now;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x000022DF File Offset: 0x000004DF
		public override string GetDescription()
		{
			return this.conDescription;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x0000222D File Offset: 0x0000042D
		public override void SendLine(string _text)
		{
		}

		// Token: 0x06000067 RID: 103 RVA: 0x0000222D File Offset: 0x0000042D
		public override void SendLines(List<string> _output)
		{
		}

		// Token: 0x06000068 RID: 104 RVA: 0x0000222D File Offset: 0x0000042D
		public override void SendLog(string _msg, string _trace, LogType _type)
		{
		}

		// Token: 0x0400002B RID: 43
		private readonly DateTime login;

		// Token: 0x0400002C RID: 44
		private DateTime lastAction;

		// Token: 0x0400002D RID: 45
		private readonly string conDescription;
	}
}
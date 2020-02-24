using System.IO;
using System.Net;
using ServerTools.WebAPI.Cache;

namespace ServerTools.WebAPI
{
	// Token: 0x0200001E RID: 30
	public class StaticHandler : PathHandler
	{
		// Token: 0x0600009E RID: 158 RVA: 0x00008FC8 File Offset: 0x000071C8
		public StaticHandler(string _staticPart, string _filePath, AbstractCache _cache, bool _logMissingFiles, string _moduleName = null) : base(_moduleName, 0)
		{
			this.staticPart = _staticPart;
			this.datapath = _filePath + ((_filePath[_filePath.Length - 1] != '/') ? "/" : string.Empty);
			this.cache = _cache;
			this.logMissingFiles = _logMissingFiles;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00009020 File Offset: 0x00007220
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			string text = _req.Url.AbsolutePath.Remove(0, this.staticPart.Length);
			byte[] fileContent = this.cache.GetFileContent(this.datapath + text);
			if (fileContent != null)
			{
				_resp.ContentType = MimeType.GetMimeType(Path.GetExtension(text));
				_resp.ContentLength64 = (long)fileContent.Length;
				_resp.OutputStream.Write(fileContent, 0, fileContent.Length);
			}
			else
			{
				_resp.StatusCode = 404;
				if (this.logMissingFiles)
				{
					Log.Out(string.Concat(new string[]
					{
						"Web:Static:FileNotFound: \"",
						_req.Url.AbsolutePath,
						"\" @ \"",
						this.datapath,
						text,
						"\""
					}));
				}
			}
		}

		// Token: 0x0400004F RID: 79
		private readonly AbstractCache cache;

		// Token: 0x04000050 RID: 80
		private readonly string datapath;

		// Token: 0x04000051 RID: 81
		private readonly bool logMissingFiles;

		// Token: 0x04000052 RID: 82
		private readonly string staticPart;
	}
}

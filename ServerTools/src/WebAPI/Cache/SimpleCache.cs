using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools.WebAPI.Cache
{
	// Token: 0x02000017 RID: 23
	public class SimpleCache : AbstractCache
	{
		// Token: 0x06000078 RID: 120 RVA: 0x00003BB4 File Offset: 0x00001DB4
		public override byte[] GetFileContent(string _filename)
		{
			try
			{
				object obj = this.fileCache;
				lock (obj)
				{
					if (!this.fileCache.ContainsKey(_filename))
					{
						if (!File.Exists(_filename))
						{
							return null;
						}
						this.fileCache.Add(_filename, File.ReadAllBytes(_filename));
					}
					return this.fileCache[_filename];
				}
			}
			catch (Exception arg)
			{
				Log.Out("Error in SimpleCache.GetFileContent: " + arg);
			}
			return null;
		}

		// Token: 0x04000036 RID: 54
		private readonly Dictionary<string, byte[]> fileCache = new Dictionary<string, byte[]>();
	}
}

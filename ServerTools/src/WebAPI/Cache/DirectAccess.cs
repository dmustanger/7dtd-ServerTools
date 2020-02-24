using System;
using System.IO;

namespace ServerTools.WebAPI.Cache
{
	// Token: 0x02000016 RID: 22
	public class DirectAccess : AbstractCache
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00003B6C File Offset: 0x00001D6C
		public override byte[] GetFileContent(string _filename)
		{
			try
			{
				if (!File.Exists(_filename))
				{
					return null;
				}
				return File.ReadAllBytes(_filename);
			}
			catch (Exception arg)
			{
				Log.Out("Error in DirectAccess.GetFileContent: " + arg);
			}
			return null;
		}
	}
}

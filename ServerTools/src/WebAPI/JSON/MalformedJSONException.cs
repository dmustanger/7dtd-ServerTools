using System;
using System.Runtime.Serialization;

namespace ServerTools.WebAPI.JSON
{
	[Serializable]
	// Token: 0x02000014 RID: 20
	public class MalformedJSONException : ApplicationException
	{
		// Token: 0x0600006F RID: 111 RVA: 0x000025E5 File Offset: 0x000007E5
		public MalformedJSONException()
		{
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000025ED File Offset: 0x000007ED
		public MalformedJSONException(string _message) : base(_message)
		{
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000025F6 File Offset: 0x000007F6
		public MalformedJSONException(string _message, Exception _inner) : base(_message, _inner)
		{
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00002600 File Offset: 0x00000800
		protected MalformedJSONException(SerializationInfo _info, StreamingContext _context) : base(_info, _context)
		{
		}
	}
}

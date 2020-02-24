
namespace ServerTools.WebAPI.JSON
{
	// Token: 0x02000012 RID: 18
	public class Parser
	{
		// Token: 0x06000069 RID: 105 RVA: 0x00003A90 File Offset: 0x00001C90
		public static JSONNode Parse(string _json)
		{
			int num = 0;
			return Parser.ParseInternal(_json, ref num);
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003AA8 File Offset: 0x00001CA8
		public static JSONNode ParseInternal(string _json, ref int _offset)
		{
			Parser.SkipWhitespace(_json, ref _offset);
			char c = _json[_offset];
			if (c == '"')
			{
				return JSONString.Parse(_json, ref _offset);
			}
			if (c != '[')
			{
				if (c != 'f')
				{
					if (c == 'n')
					{
						return JSONNull.Parse(_json, ref _offset);
					}
					if (c != 't')
					{
						if (c != '{')
						{
							return JSONNumber.Parse(_json, ref _offset);
						}
						return JSONObject.Parse(_json, ref _offset);
					}
				}
				return JSONBoolean.Parse(_json, ref _offset);
			}
			return JSONArray.Parse(_json, ref _offset);
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00003B14 File Offset: 0x00001D14
		public static void SkipWhitespace(string _json, ref int _offset)
		{
			while (_offset < _json.Length)
			{
				char c = _json[_offset];
				switch (c)
				{
					case '\t':
					case '\n':
					case '\r':
						break;
					default:
						if (c != ' ')
						{
							return;
						}
						break;
				}
				_offset++;
			}
			throw new MalformedJSONException("End of JSON reached before parsing finished");
		}
	}
}

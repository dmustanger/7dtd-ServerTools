using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x0200000F RID: 15
	public class JSONString : JSONValue
	{
		// Token: 0x0600005C RID: 92 RVA: 0x00002544 File Offset: 0x00000744
		public JSONString(string _value)
		{
			this.value = _value;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00002553 File Offset: 0x00000753
		public string GetString()
		{
			return this.value;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000036C0 File Offset: 0x000018C0
		public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
		{
			if (this.value != null && this.value.Length != 0)
			{
				int length = this.value.Length;
				_stringBuilder.EnsureCapacity(_stringBuilder.Length + 2 * length);
				_stringBuilder.Append('"');
				string text = this.value;
				int i = 0;
				while (i < text.Length)
				{
					char c = text[i];
					switch (c)
					{
						case '\b':
							_stringBuilder.Append("\\b");
							break;
						case '\t':
							_stringBuilder.Append("\\t");
							break;
						case '\n':
							_stringBuilder.Append("\\n");
							break;
						case '\v':
							goto IL_A6;
						case '\f':
							_stringBuilder.Append("\\f");
							break;
						case '\r':
							_stringBuilder.Append("\\r");
							break;
						default:
							goto IL_A6;
					}
				IL_110:
					i++;
					continue;
				IL_A6:
					if (c == '"' || c == '\\')
					{
						_stringBuilder.Append('\\');
						_stringBuilder.Append(c);
						goto IL_110;
					}
					if (c < ' ')
					{
						_stringBuilder.Append("\\u");
						int num = (int)c;
						_stringBuilder.Append(num.ToString("X4"));
						goto IL_110;
					}
					_stringBuilder.Append(c);
					goto IL_110;
				}
				_stringBuilder.Append('"');
				return;
			}
			_stringBuilder.Append("\"\"");
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00003804 File Offset: 0x00001A04
		public static JSONString Parse(string _json, ref int _offset)
		{
			StringBuilder stringBuilder = new StringBuilder();
			_offset++;
			while (_offset < _json.Length)
			{
				char c = _json[_offset];
				if (c != '\\')
				{
					if (c == '"')
					{
						_offset++;
						return new JSONString(stringBuilder.ToString());
					}
					stringBuilder.Append(_json[_offset]);
					_offset++;
				}
				else
				{
					_offset++;
					char c2 = _json[_offset];
					switch (c2)
					{
						case 'r':
							stringBuilder.Append('\r');
							break;
						case 's':
							goto IL_72;
						case 't':
							stringBuilder.Append('\t');
							break;
						default:
							goto IL_72;
					}
				IL_DB:
					_offset++;
					continue;
				IL_72:
					if (c2 == '"' || c2 == '/' || c2 == '\\')
					{
						stringBuilder.Append(_json[_offset]);
						goto IL_DB;
					}
					if (c2 == 'b')
					{
						stringBuilder.Append('\b');
						goto IL_DB;
					}
					if (c2 == 'f')
					{
						stringBuilder.Append('\f');
						goto IL_DB;
					}
					if (c2 != 'n')
					{
						stringBuilder.Append(_json[_offset]);
						goto IL_DB;
					}
					stringBuilder.Append('\n');
					goto IL_DB;
				}
			}
			throw new MalformedJSONException("End of JSON reached before parsing string finished");
		}

		// Token: 0x04000032 RID: 50
		private readonly string value;
	}
}
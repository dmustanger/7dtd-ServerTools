using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x02000010 RID: 16
	public class JSONBoolean : JSONValue
	{
		// Token: 0x06000060 RID: 96 RVA: 0x0000255B File Offset: 0x0000075B
		public JSONBoolean(bool _value)
		{
			this.value = _value;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x0000256A File Offset: 0x0000076A
		public bool GetBool()
		{
			return this.value;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002572 File Offset: 0x00000772
		public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
		{
			_stringBuilder.Append((!this.value) ? "false" : "true");
		}

		// Token: 0x06000063 RID: 99 RVA: 0x0000391C File Offset: 0x00001B1C
		public static JSONBoolean Parse(string _json, ref int _offset)
		{
			if (_json.Substring(_offset, 4).Equals("true"))
			{
				_offset += 4;
				return new JSONBoolean(true);
			}
			if (!_json.Substring(_offset, 5).Equals("false"))
			{
				throw new MalformedJSONException("No valid boolean found");
			}
			_offset += 5;
			return new JSONBoolean(false);
		}

		// Token: 0x04000033 RID: 51
		private readonly bool value;
	}
}
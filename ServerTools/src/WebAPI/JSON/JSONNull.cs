using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x02000013 RID: 19
	public class JSONNull : JSONValue
	{
		// Token: 0x0600006D RID: 109 RVA: 0x000025AA File Offset: 0x000007AA
		public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
		{
			_stringBuilder.Append("null");
		}

		// Token: 0x0600006E RID: 110 RVA: 0x000025B8 File Offset: 0x000007B8
		public static JSONNull Parse(string _json, ref int _offset)
		{
			if (!_json.Substring(_offset, 4).Equals("null"))
			{
				throw new MalformedJSONException("No valid null value found");
			}
			_offset += 4;
			return new JSONNull();
		}
	}
}
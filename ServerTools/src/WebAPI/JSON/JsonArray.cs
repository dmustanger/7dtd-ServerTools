using System.Collections.Generic;
using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x0200000C RID: 12
	public class JSONArray : JSONNode
	{
		// Token: 0x1700001D RID: 29
		public JSONNode this[int _index]
		{
			get
			{
				return this.nodes[_index];
			}
			set
			{
				this.nodes[_index] = value;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00002484 File Offset: 0x00000684
		public int Count
		{
			get
			{
				return this.nodes.Count;
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00002491 File Offset: 0x00000691
		public void Add(JSONNode _node)
		{
			this.nodes.Add(_node);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003180 File Offset: 0x00001380
		public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
		{
			_stringBuilder.Append("[");
			if (_prettyPrint)
			{
				_stringBuilder.Append('\n');
			}
			foreach (JSONNode jsonnode in this.nodes)
			{
				if (_prettyPrint)
				{
					_stringBuilder.Append(new string('\t', _currentLevel + 1));
				}
				jsonnode.ToString(_stringBuilder, _prettyPrint, _currentLevel + 1);
				_stringBuilder.Append(",");
				if (_prettyPrint)
				{
					_stringBuilder.Append('\n');
				}
			}
			if (this.nodes.Count > 0)
			{
				_stringBuilder.Remove(_stringBuilder.Length - ((!_prettyPrint) ? 1 : 2), 1);
			}
			if (_prettyPrint)
			{
				_stringBuilder.Append(new string('\t', _currentLevel));
			}
			_stringBuilder.Append("]");
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003260 File Offset: 0x00001460
		public static JSONArray Parse(string _json, ref int _offset)
		{
			JSONArray jsonarray = new JSONArray();
			bool flag = true;
			_offset++;
			for (; ; )
			{
				Parser.SkipWhitespace(_json, ref _offset);
				char c = _json[_offset];
				if (c != ',')
				{
					if (c == ']')
					{
						goto IL_53;
					}
					jsonarray.Add(Parser.ParseInternal(_json, ref _offset));
					flag = false;
				}
				else
				{
					if (flag)
					{
						break;
					}
					flag = true;
					_offset++;
				}
			}
			throw new MalformedJSONException("Could not parse array, found a comma without a value first");
		IL_53:
			_offset++;
			return jsonarray;
		}

		// Token: 0x0400002F RID: 47
		private readonly List<JSONNode> nodes = new List<JSONNode>();
	}
}
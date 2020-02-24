using System.Collections.Generic;
using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x0200000D RID: 13
	public class JSONObject : JSONNode
	{
		// Token: 0x1700001F RID: 31
		public JSONNode this[string _name]
		{
			get
			{
				return this.nodes[_name];
			}
			set
			{
				this.nodes[_name] = value;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000024CF File Offset: 0x000006CF
		public int Count
		{
			get
			{
				return this.nodes.Count;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000052 RID: 82 RVA: 0x000024DC File Offset: 0x000006DC
		public List<string> Keys
		{
			get
			{
				return new List<string>(this.nodes.Keys);
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000024EE File Offset: 0x000006EE
		public bool ContainsKey(string _name)
		{
			return this.nodes.ContainsKey(_name);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000024FC File Offset: 0x000006FC
		public void Add(string _name, JSONNode _node)
		{
			this.nodes.Add(_name, _node);
		}

		// Token: 0x06000055 RID: 85 RVA: 0x000032C8 File Offset: 0x000014C8
		public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
		{
			_stringBuilder.Append("{");
			if (_prettyPrint)
			{
				_stringBuilder.Append('\n');
			}
			foreach (KeyValuePair<string, JSONNode> keyValuePair in this.nodes)
			{
				if (_prettyPrint)
				{
					_stringBuilder.Append(new string('\t', _currentLevel + 1));
				}
				_stringBuilder.Append(string.Format("\"{0}\":", keyValuePair.Key));
				if (_prettyPrint)
				{
					_stringBuilder.Append(" ");
				}
				keyValuePair.Value.ToString(_stringBuilder, _prettyPrint, _currentLevel + 1);
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
			_stringBuilder.Append("}");
		}

		// Token: 0x06000056 RID: 86 RVA: 0x000033D4 File Offset: 0x000015D4
		public static JSONObject Parse(string _json, ref int _offset)
		{
			JSONObject jsonobject = new JSONObject();
			bool flag = true;
			_offset++;
			for (; ; )
			{
				Parser.SkipWhitespace(_json, ref _offset);
				char c = _json[_offset];
				if (c != '"')
				{
					if (c != ',')
					{
						if (c == '}')
						{
							goto IL_A3;
						}
					}
					else
					{
						if (flag)
						{
							goto IL_82;
						}
						flag = true;
						_offset++;
					}
				}
				else
				{
					if (!flag)
					{
						goto IL_98;
					}
					JSONString jsonstring = JSONString.Parse(_json, ref _offset);
					Parser.SkipWhitespace(_json, ref _offset);
					if (_json[_offset] != ':')
					{
						break;
					}
					_offset++;
					JSONNode node = Parser.ParseInternal(_json, ref _offset);
					jsonobject.Add(jsonstring.GetString(), node);
					flag = false;
				}
			}
			throw new MalformedJSONException("Could not parse object, missing colon (\":\") after key");
		IL_82:
			throw new MalformedJSONException("Could not parse object, found a comma without a key/value pair first");
		IL_98:
			throw new MalformedJSONException("Could not parse object, found new key without a separating comma");
		IL_A3:
			_offset++;
			return jsonobject;
		}

		// Token: 0x04000030 RID: 48
		private readonly Dictionary<string, JSONNode> nodes = new Dictionary<string, JSONNode>();
	}
}

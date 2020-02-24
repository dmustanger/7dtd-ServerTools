using System;
using System.Globalization;
using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x0200000E RID: 14
	public class JSONNumber : JSONValue
	{
		// Token: 0x06000057 RID: 87 RVA: 0x0000250B File Offset: 0x0000070B
		public JSONNumber(double _value)
		{
			this.value = _value;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x0000251A File Offset: 0x0000071A
		public double GetDouble()
		{
			return this.value;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00002522 File Offset: 0x00000722
		public int GetInt()
		{
			return (int)Math.Round(this.value);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00002530 File Offset: 0x00000730
		public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
		{
			_stringBuilder.Append(this.value.ToCultureInvariantString());
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000348C File Offset: 0x0000168C
		public static JSONNumber Parse(string _json, ref int _offset)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = null;
			bool flag = false;
			bool flag2 = false;
			while (_offset < _json.Length)
			{
				if (_json[_offset] >= '0' && _json[_offset] <= '9')
				{
					if (flag2)
					{
						stringBuilder2.Append(_json[_offset]);
					}
					else
					{
						stringBuilder.Append(_json[_offset]);
					}
				}
				else if (_json[_offset] == '.')
				{
					if (flag2)
					{
						throw new MalformedJSONException("Decimal separator in exponent");
					}
					if (flag)
					{
						throw new MalformedJSONException("Multiple decimal separators in number found");
					}
					if (stringBuilder.Length == 0)
					{
						throw new MalformedJSONException("No leading digits before decimal separator found");
					}
					stringBuilder.Append('.');
					flag = true;
				}
				else if (_json[_offset] == '-')
				{
					if (flag2)
					{
						if (stringBuilder2.Length > 0)
						{
							throw new MalformedJSONException("Negative sign in exponent after digits");
						}
						stringBuilder2.Append(_json[_offset]);
					}
					else
					{
						if (stringBuilder.Length > 0)
						{
							throw new MalformedJSONException("Negative sign in mantissa after digits");
						}
						stringBuilder.Append(_json[_offset]);
					}
				}
				else
				{
					if (_json[_offset] != 'e')
					{
						if (_json[_offset] != 'E')
						{
							if (_json[_offset] == '+')
							{
								if (!flag2)
								{
									throw new MalformedJSONException("Positive sign in mantissa found");
								}
								if (stringBuilder2.Length <= 0)
								{
									stringBuilder2.Append(_json[_offset]);
									goto IL_132;
								}
								throw new MalformedJSONException("Positive sign in exponent after digits");
							}
							else
							{
								double num;
								if (!StringParsers.TryParseDouble(stringBuilder.ToString(), out num, 0, -1, NumberStyles.Any))
								{
									throw new MalformedJSONException("Mantissa is not a valid decimal (\"" + stringBuilder + "\")");
								}
								if (flag2)
								{
									int num2;
									if (!int.TryParse(stringBuilder2.ToString(), out num2))
									{
										throw new MalformedJSONException("Exponent is not a valid integer (\"" + stringBuilder2 + "\")");
									}
									num *= Math.Pow(10.0, (double)num2);
								}
								return new JSONNumber(num);
							}
						}
					}
					if (flag2)
					{
						throw new MalformedJSONException("Multiple exponential markers in number found");
					}
					if (stringBuilder.Length == 0)
					{
						throw new MalformedJSONException("No leading digits before exponential marker found");
					}
					stringBuilder2 = new StringBuilder();
					flag2 = true;
				}
			IL_132:
				_offset++;
			}
			throw new MalformedJSONException("End of JSON reached before parsing number finished");
		}

		// Token: 0x04000031 RID: 49
		private readonly double value;
	}
}

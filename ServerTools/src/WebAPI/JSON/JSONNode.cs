using System.Text;

namespace ServerTools.WebAPI.JSON
{
	// Token: 0x0200000B RID: 11
	public abstract class JSONNode
	{
		// Token: 0x06000045 RID: 69
		public abstract void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0);

		// Token: 0x06000046 RID: 70 RVA: 0x0000315C File Offset: 0x0000135C
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			this.ToString(stringBuilder, false, 0);
			return stringBuilder.ToString();
		}
	}
}
using System.Data;
using System.Text.RegularExpressions;

namespace ServerTools
{
    public class SQL
    {
        public static int Sql_version = 2;

        public static void Connect()
        {
            SQLiteDatabase.SetConnection();
        }

        public static void FastQuery(string _sql, string _class)
        {
            SQLiteDatabase.FastQuery(_sql, _class);
        }

        public static DataTable TQuery(string _sql)
        {
            DataTable dt = new DataTable();
            dt = SQLiteDatabase.TypeQuery(_sql);
            return dt;
        }

        public static string EscapeString(string _string)
        {
            string _str = Regex.Escape(_string);
            return _str;
        }
    }
}

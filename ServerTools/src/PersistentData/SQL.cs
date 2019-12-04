using System.Data;
using System.Text.RegularExpressions;

namespace ServerTools
{
    public class SQL
    {
        public static int Sql_version = 3;

        public static void Connect()
        {
            if (!MySqlDB.IsEnabled)
            {
                SQLiteDb.SetConnection();
            }
            else
            {
                //MySqlDB.SetConnection();
            }
        }

        public static void FastQuery(string _sql, string _class)
        {
            if (!MySqlDB.IsEnabled)
            {
                SQLiteDb.FastQuery(_sql, _class);
            }
            else
            {
                //MySqlDB.FastQuery(_sql, _class);
            }
        }

        public static DataTable TypeQuery(string _sql)
        {
            DataTable dt = new DataTable();
            if (!MySqlDB.IsEnabled)
            {
                dt = SQLiteDb.TypeQuery(_sql);
            }
            else
            {
                //dt = MySqlDB.TypeQuery(_sql);
            }
            return dt;
        }

        public static string EscapeString(string _string)
        {
            string _str = Regex.Escape(_string);
            return _str;
        }
    }
}

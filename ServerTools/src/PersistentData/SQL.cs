using System.Data;

namespace ServerTools
{
    public class SQL
    {
        public static int Sql_version = 6;
        public static bool IsMySql = false;

        public static void Connect()
        {
            if (IsMySql)
            {
                Log.Out("[SERVERTOOLS] Using MySQL for persistence");
                MySqlDatabase.SetConnection();
            }
            else
            {
                Log.Out("[SERVERTOOLS] Using Sqlite for persistence");
                SQLiteDatabase.SetConnection();
            }
        }

        public static void FastQuery(string _sql)
        {
            if (IsMySql)
            {
                MySqlDatabase.FastQuery(_sql);
            }
            else
            {
                SQLiteDatabase.FastQuery(_sql);
            }
        }

        public static DataTable TQuery(string _sql)
        {
            DataTable dt = new DataTable();
            if (IsMySql)
            {
                dt = MySqlDatabase.TQuery(_sql);
            }
            else
            {
                dt = SQLiteDatabase.TQuery(_sql);
            }
            return dt;
        }

        public static string EscapeString(string _string)
        {
            string _str = MySqlDatabase.EscapeString(_string);
            return _str;
        }
    }
}

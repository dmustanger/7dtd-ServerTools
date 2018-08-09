using System.Data;

namespace ServerTools
{
    public class SQL
    {
        private static bool IsMySql = false;

        public static void Connect()
        {
            if (IsMySql)
            {
                MySqlDatabase.SetConnection();
            }
            else
            {
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
    }
}

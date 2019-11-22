using System;
using System.Data;

namespace ServerTools
{
    public class UpdateSQL
    {
        public static void Exec(int _version)
        {
            try
            {
                if (_version == 1)
                {
                    SQLiteDatabase.FastQuery("ALTER TABLE Hardcore ADD extraLives INTEGER DEFAULT 0;", "UpdateSQL");
                }
                //SQL.FastQuery("UPDATE Config SET sql_version = 2 WHERE sql_version = 1", "UpdateSQL");
                CheckVersion();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in UpdateSQL.Exec: {0}.", e));
            }
        }

        private static void CheckVersion()
        {
            DataTable _result = SQL.TQuery("SELECT sql_version FROM Config");
            int _version;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _version);
            _result.Dispose();
            if (_version != SQL.Sql_version)
            {
                Exec(_version);
            }
            else
            {
                LoadProcess.Load(4);
            }
        }
    }
}

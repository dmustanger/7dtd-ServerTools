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
                    if (!MySqlDB.IsEnabled)
                    {
                        SQL.FastQuery("ALTER TABLE Hardcore ADD extraLives INTEGER DEFAULT 0;", "UpdateSQL");
                    }
                    else
                    {
                        SQL.FastQuery("ALTER TABLE Hardcore ADD extraLives INT DEFAULT 0;", "UpdateSQL");
                    }
                    SQL.FastQuery("UPDATE Config SET sql_version = 2 WHERE sql_version = 1", "UpdateSQL");
                }
                else if (_version == 2)
                {
                    if (!MySqlDB.IsEnabled)
                    {
                        SQL.FastQuery("ALTER TABLE Hardcore ADD oldDeaths INTEGER DEFAULT 0;", "UpdateSQL");
                    }
                    else
                    {
                        SQL.FastQuery("ALTER TABLE Hardcore ADD oldDeaths INT DEFAULT 0;", "UpdateSQL");
                    }
                    SQL.FastQuery("UPDATE Config SET sql_version = 3 WHERE sql_version = 2", "UpdateSQL");
                }
                CheckVersion();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in UpdateSQL.Exec: {0}.", e));
            }
        }

        private static void CheckVersion()
        {
            try
            {
                DataTable _result = SQL.TypeQuery("SELECT sql_version FROM Config");
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in UpdateSQL.CheckVersion: {0}.", e));
            }
        }
    }
}

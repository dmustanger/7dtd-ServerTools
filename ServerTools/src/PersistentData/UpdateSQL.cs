using System.Data;

namespace ServerTools
{
    public class UpdateSQL
    {
        public static void Exec(int _version)
        {
            if (_version == 1)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns MODIFY eventSpawn VARCHAR(50) DEFAULT NULL; ALTER TABLE EventSpawns MODIFY eventRespawn VARCHAR(50) DEFAULT NULL");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD return VARCHAR(50) DEFAULT 'false';");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD eventRespawn VARCHAR(50) DEFAULT 'false';");
                }
                else
                {
                    SQLiteDatabase.FastQuery("ALTER TABLE EventSpawns MODIFY eventSpawn TEXT DEFAULT NULL; ALTER TABLE EventSpawns MODIFY eventRespawn TEXT DEFAULT NULL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD return TEXT DEFAULT 'false';");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD eventRespawn TEXT DEFAULT 'false';");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 2 WHERE sql_version = 1");
            }
            CheckVersion();
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
        }
    }
}

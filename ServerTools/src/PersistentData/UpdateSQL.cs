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
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD return VARCHAR(50) DEFAULT 'false'; ALTER TABLE Players ADD eventRespawn VARCHAR(50) DEFAULT 'false'; ALTER TABLE Players ADD eventSpawn VARCHAR(50) DEFAULT 'false'; ALTER TABLE Players ADD newSpawn VARCHAR(50) DEFAULT 'false';");
                }
                else
                {
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD return TEXT DEFAULT 'false'; ALTER TABLE Players ADD eventRespawn TEXT DEFAULT 'false'; ALTER TABLE Players ADD eventSpawn TEXT DEFAULT 'false'; ALTER TABLE Players ADD newSpawn TEXT DEFAULT 'false';");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 2 WHERE sql_version = 1");
            }
            if (_version == 2)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns DROP eventSpawn; ALTER TABLE EventSpawns DROP eventRespawn;");
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns ADD eventSpawn VARCHAR(50); ALTER TABLE EventSpawns ADD eventRespawn VARCHAR(50);");
                }
                else
                {
                    SQLiteDatabase.FastQuery("DROP TABLE EventSpawns");
                    SQLiteDatabase.FastQuery("CREATE TABLE IF NOT EXISTS EventSpawns (eventid INTEGER NOT NULL, eventTeam INTEGER NOT NULL, eventSpawn TEXT, eventRespawn TEXT, FOREIGN KEY(eventid) REFERENCES Events(eventid))");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 3 WHERE sql_version = 2");
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

using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ServerTools
{
    public class SQLiteDatabase
    {
        private static SQLiteConnection connection;
        private static SQLiteCommand cmd;

        public static void SetConnection()
        {
            string _dbConnection;
            string _filepath = string.Format("{0}/ServerTools.db", GameUtils.GetSaveGameDir());
            if (File.Exists(_filepath))
            {
                _dbConnection = string.Format("Data Source={0};Version=3;New=False;Compress=True;", _filepath);
            }
            else
            {
                _dbConnection = string.Format("Data Source={0};Version=3;New=True;Compress=True;", _filepath);
            }
            connection = new SQLiteConnection(_dbConnection);
            CreateTables();
        }


        private static void CreateTables()
        {
            FastQuery("CREATE TABLE IF NOT EXISTS Players (" +
                "steamid TEXT PRIMARY KEY ON CONFLICT REPLACE, " +
                "playername TEXT DEFAULT 'Unknown', " +
                "last_joined TEXT DEFAULT 'Never', " +
                "pingimmunity TEXT DEFAULT 'false', " +
                "last_gimme TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastkillme TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "bank INTEGER DEFAULT 0, " +
                "wallet INTEGER DEFAULT 0, " +
                "playerSpentCoins INTEGER DEFAULT 0, " +
                "hardcoreSessionTime INTEGER DEFAULT 0, " +
                "hardcoreKills INTEGER DEFAULT 0, " +
                "hardcoreZKills INTEGER DEFAULT 0, " +
                "hardcoreScore INTEGER DEFAULT 0, " +
                "hardcoreDeaths INTEGER DEFAULT 0, " +
                "hardcoreName TEXT DEFAULT 'Unknown', " +
                "bounty INTEGER DEFAULT 0, " +
                "bountyHunter INTEGER DEFAULT 0," +
                "sessionTime INTEGER DEFAULT 0" +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS Config (sql_version INTEGER);");
            DataTable _result = SQL.TQuery("SELECT sql_version FROM Config");
            if (_result.Rows.Count == 0)
            {
                string _sql = string.Format("INSERT INTO Config (sql_version) VALUES ({0})", SQL.Sql_version);
                SQL.FastQuery(_sql);
            }
            else
            {
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _version);
                if (_version != SQL.Sql_version)
                {
                    SQL.UpdateSQL(_version);
                }
            }
        }

        public static DataTable TQuery(string _sql)
        {
            DataTable dt = new DataTable();
            try
            {
                connection.Open();
                cmd = new SQLiteCommand(_sql, connection);
                SQLiteDataReader _reader = cmd.ExecuteReader();
                dt.Load(_reader);
                _reader.Close();
                connection.Close();
            }
            catch (SQLiteException e)
            {
                Log.Out(string.Format("[ServerTools] SQLiteException in SQLiteDatabase.TQuery: {0}", e));
            }
            return dt;
        }

        public static void FastQuery(string _sql)
        {
            try
            {
                connection.Open();
                cmd = new SQLiteCommand(_sql, connection);
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch (SQLiteException e)
            {
                Log.Out(string.Format("[ServerTools] SQLiteException in SQLiteDatabase.FastQuery: {0}", e));
            }
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace ServerTools
{
    public class SQLiteDatabase
    {
        public static bool FastQ = false, TypeQ = false;
        private static SQLiteConnection connection;
        private static SQLiteCommand cmd;
        private static Queue<string> FQuery = new Queue<string>();

        public static void SetConnection()
        {
            string _dbConnection;
            string _filepath = string.Format("{0}/ServerTools.db", GameUtils.GetSaveGameDir());
            if (File.Exists(_filepath))
            {
                _dbConnection = string.Format("Data Source={0};Version=3;New=False;Compress=True;Pooling=false;", _filepath);
            }
            else
            {
                _dbConnection = string.Format("Data Source={0};Version=3;New=True;Compress=True;Pooling=false;", _filepath);
            }
            connection = new SQLiteConnection(_dbConnection);
            CreateTables();
        }


        private static void CreateTables()
        {
            FastQuery("CREATE TABLE IF NOT EXISTS Players (" +
                "steamid TEXT PRIMARY KEY ON CONFLICT REPLACE, " +
                "playername TEXT DEFAULT 'Unknown', " +
                "countryban TEXT DEFAULT 'false', " +
                "return TEXT DEFAULT 'false', " +
                "eventReturn TEXT DEFAULT 'false', " +
                "eventRespawn TEXT DEFAULT 'false', " +
                "eventSpawn TEXT DEFAULT 'false', " +
                "extraLives INTEGER DEFAULT 0, " +
                "lastWaypoint TEXT DEFAULT '10/29/2000 7:30:00 AM' " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS Waypoints (" +
                "wayPointid INTEGER PRIMARY KEY, " +
                "steamid TEXT NOT NULL, " +
                "wayPointName TEXT NOT NULL, " +
                "position TEXT NOT NULL " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS Polls (" +
                "pollid INTEGER PRIMARY KEY, " +
                "pollOpen TEXT DEFAULT 'false', " +
                "pollTime TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "pollHours INTEGER NOT NULL, " +
                "pollMessage TEXT NOT NULL, " +
                "pollYes INTEGER DEFAULT 0, " +
                "pollNo INTEGER DEFAULT 0 " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS Events (" +
                "eventid INTEGER PRIMARY KEY, " +
                "eventAdmin TEXT, " +
                "eventName TEXT, " +
                "eventInvite TEXT, " +
                "eventTeams INTEGER, " +
                "eventPlayerCount INTEGER, " +
                "eventTime INTEGER, " +
                "eventActive TEXT " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS EventSpawns (" +
                "eventid INTEGER NOT NULL, " +
                "eventTeam INTEGER NOT NULL, " +
                "eventSpawn TEXT, " +
                "eventRespawn TEXT, " +
                "FOREIGN KEY(eventid) REFERENCES Events(eventid) " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS Hardcore (" +
                "eventid INTEGER PRIMARY KEY, " +
                "steamid TEXT NOT NULL, " +
                "sessionTime INTEGER DEFAULT 0, " +
                "kills INTEGER DEFAULT 0, " +
                "zKills INTEGER DEFAULT 0, " +
                "score INTEGER DEFAULT 0, " +
                "deaths INTEGER DEFAULT 0, " +
                "playerName TEXT NOT NULL " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS Tracking (" +
                "trackid INTEGER PRIMARY KEY, " +
                "dateTime TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "position TEXT NOT NULL, " +
                "steamId TEXT NOT NULL, " +
                "playerName TEXT NOT NULL, " +
                "holding TEXT NOT NULL " +
                ");", "SQLiteDatabase");
            FastQuery("CREATE TABLE IF NOT EXISTS Config (sql_version INTEGER);", "SQLiteDatabase");
            int _version = 1;
            DataTable _result = SQL.TQuery("SELECT sql_version FROM Config");
            if (_result.Rows.Count == 0)
            {
                FastQuery("INSERT INTO Config (sql_version) VALUES (1)", "SQLiteDatabase");
            }
            else
            {
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _version);
            }
            _result.Dispose();
            if (_version != SQL.Sql_version)
            {
                //UpdateSQL.Exec(_version);
            }
        }

        public static DataTable TypeQuery(string _sql)
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

        public static void FastQuery(string _sql, string _class)
        {
            FQuery.Enqueue(_sql);
            if (!FastQ)
            {
                try
                {
                    FastQ = true;
                    connection.Open(); 
                    cmd = new SQLiteCommand(FQuery.ElementAt(0), connection);
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    Log.Out(string.Format("[ServerTools] SQLiteException in SQLiteDatabase.FastQuery: {0}", e));
                }
                FQuery.Dequeue();
                if (FQuery.Count > 0)
                {
                    cmd = new SQLiteCommand(FQuery.ElementAt(0), connection);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    connection.Close();
                    FastQ = false;
                }
            }
        }
    }
}

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
            // can remove the below a few months after release
            string _binpath = string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir());
            if (File.Exists(_binpath))
            {
                StateManager.Awake();
                UpdateToSqlFromBin.Exec();
            }
            //----------------------------------------------------------------------------------------
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
                "bountyHunter INTEGER DEFAULT 0, " +
                "sessionTime INTEGER DEFAULT 0, " +
                "bikeId INTEGER DEFAULT 0, " +
                "lastBike TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "jailTime INTEGER DEFAULT 0, " +
                "jailName TEXT DEFAULT 'Unknown', " +
                "jailDate TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "muteTime INTEGER DEFAULT 0, " +
                "muteName TEXT DEFAULT 'Unknown', " +
                "muteDate TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "zkills INTEGER DEFAULT 0, " +
                "kills INTEGER DEFAULT 0, " +
                "deaths INTEGER DEFAULT 0, " +
                "eventReturn TEXT DEFAULT 'Unknown', " +
                "marketReturn TEXT DEFAULT 'Unknown', " +
                "lobbyReturn TEXT DEFAULT 'Unknown', " +
                "newTeleSpawn TEXT DEFAULT 'Unknown', " +
                "homeposition TEXT DEFAULT 'Unknown', " +
                "homeposition2 TEXT DEFAULT 'Unknown', " +
                "lastsethome TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastwhisper TEXT DEFAULT 'Unknown', " +
                "lastWaypoint TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastMarket TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastStuck TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastLobby TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastLog TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastDied TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastFriendTele TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "respawnTime TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastTravel TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastAnimals TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "lastVoteReward TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "firstClaim TEXT DEFAULT 'false', " +
                "ismuted TEXT DEFAULT 'false', " +
                "isjailed TEXT DEFAULT 'false', " +
                "startingItems TEXT DEFAULT 'false', " +
                "clanname TEXT DEFAULT 'Unknown', " +
                "invitedtoclan TEXT DEFAULT 'Unknown', " +
                "isclanowner TEXT DEFAULT 'false', " +
                "isclanofficer TEXT DEFAULT 'false', " +
                "customCommand1 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand2 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand3 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand4 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand5 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand6 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand7 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand8 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand9 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "customCommand10 TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "countryban TEXT DEFAULT 'false' " +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS Auction (" +
                "auctionid INTEGER PRIMARY KEY, " +
                "steamid TEXT NOT NULL, " +
                "itemName TEXT NOT NULL, " +
                "itemCount INTEGER NOT NULL, " +
                "itemQuality INTEGER NOT NULL, " +
                "itemPrice INTEGER NOT NULL, " +
                "cancelTime TEXT DEFAULT '10/29/2000 7:30:00 AM' " +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS Waypoints (" +
                "wayPointid INTEGER PRIMARY KEY, " +
                "steamid TEXT NOT NULL, " +
                "wayPointName TEXT NOT NULL, " +
                "position TEXT NOT NULL " +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS Polls (" +
                "pollid INTEGER PRIMARY KEY, " +
                "pollOpen TEXT DEFAULT 'false', " +
                "pollTime TEXT DEFAULT '10/29/2000 7:30:00 AM', " +
                "pollHours INTEGER NOT NULL, " +
                "pollMessage TEXT NOT NULL, " +
                "pollYes INTEGER DEFAULT 0, " +
                "pollNo INTEGER DEFAULT 0 " +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS Events (" +
                "eventid INTEGER PRIMARY KEY, " +
                "eventAdmin TEXT, " +
                "eventName TEXT, " +
                "eventInvite TEXT, " +
                "eventTeams INTEGER, " +
                "eventPlayerCount INTEGER, " +
                "eventTime INTEGER, " +
                "eventActive TEXT " +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS EventSpawns (" +
                "eventid INTEGER NOT NULL, " +
                "eventTeam INTEGER NOT NULL, " +
                "eventSpawn TEXT, " +
                "eventRespawn TEXT, " +
                "FOREIGN KEY(eventid) REFERENCES Events(eventid) " +
                ");");
            FastQuery("CREATE TABLE IF NOT EXISTS Config (sql_version INTEGER);");
            int _version = 1;
            DataTable _result = SQL.TQuery("SELECT sql_version FROM Config");
            if (_result.Rows.Count == 0)
            {
                FastQuery("INSERT INTO Config (sql_version) VALUES (1)");
            }
            else
            {
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _version);
            }
            _result.Dispose();
            if (_version != SQL.Sql_version)
            {
                UpdateSQL.Exec(_version);
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
            }
            catch (SQLiteException e)
            {
                Log.Out(string.Format("[ServerTools] SQLiteException in SQLiteDatabase.TQuery: {0}", e));
            }
            connection.Close();
            return dt;
        }

        public static void FastQuery(string _sql)
        {
            try
            {
                connection.Open();
                cmd = new SQLiteCommand(_sql, connection);
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                Log.Out(string.Format("[ServerTools] SQLiteException in SQLiteDatabase.FastQuery: {0}", e));
            }
            connection.Close();
        }
    }
}

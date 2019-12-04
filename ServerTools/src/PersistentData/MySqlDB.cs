//using MySql.Data.MySqlClient;
//using System.Data;

namespace ServerTools
{
    class MySqlDB
    {
        public static bool IsEnabled = false;
        public static string Server = "localhost", Database = "mysql", UserName = "UserName", Password = "ChangeMe";
        public static int Port = 3306;
        //private static MySqlConnection connection;
        //private static MySqlCommand cmd;

        //public static void SetConnection()
        //{
        //    string _connectionString = string.Format("Server={0}; port={1}; database={2}; UID={3}; password={4}; charset=utf8;", Server, Port, Database, UserName, Password);
        //    connection = new MySqlConnection(_connectionString);
        //    try
        //    {
        //        Log.Out("[ServerTools] Connecting to MySql Database.");
        //        connection.Open();
        //    }
        //    catch (MySqlException e)
        //    {
        //        switch (e.Number)
        //        {
        //            case 0:
        //            Log.Out("[ServerTools] MySqlException in MySqlDB.SetConnection: Cannot connect to server.");
        //            break;
        //
        //            case 1045:
        //            Log.Out("[ServerTools] MySqlException in MySqlDB.SetConnection: Invalid username/password, please try again.");
        //            break;
        //        }
        //        return;
        //    }
        //    try
        //    {
        //        connection.Close();
        //        CreateTables();
        //    }
        //    catch (MySqlException e)
        //    {
        //        Log.Out(string.Format("[ServerTools] MySqlException in MySqlDB.SetConnection: {0}", e));
        //    }
        //}
        //
        //private static void CreateTables()
        //{
        //    try
        //    {
        //        FastQuery("CREATE TABLE IF NOT EXISTS Players (" +
        //            "steamid VARCHAR(20) PRIMARY KEY ON CONFLICT REPLACE, " +
        //            "playername VARCHAR(50) DEFAULT 'Unknown', " +
        //            "countryban VARCHAR(10) DEFAULT 'false', " +
        //            "return VARCHAR(10) DEFAULT 'false', " +
        //            "eventReturn VARCHAR(10) DEFAULT 'false', " +
        //            "eventRespawn VARCHAR(10) DEFAULT 'false', " +
        //            "eventSpawn VARCHAR(10) DEFAULT 'false', " +
        //            "extraLives VARCHAR(10) NOT NULL, " +
        //            "lastWaypoint VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM' " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS Waypoints (" +
        //            "wayPointid INT PRIMARY KEY, " +
        //            "steamid VARCHAR(20) NOT NULL, " +
        //            "wayPointName VARCHAR(255) NOT NULL, " +
        //            "position VARCHAR(10) NOT NULL " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS Polls (" +
        //            "pollid INTEGER PRIMARY KEY, " +
        //            "pollOpen VARCHAR(10) DEFAULT 'false', " +
        //            "pollTime VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM', " +
        //            "pollHours INT NOT NULL, " +
        //            "pollMessage VARCHAR(255) NOT NULL, " +
        //            "pollYes INT DEFAULT 0, " +
        //            "pollNo INT DEFAULT 0 " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS Events (" +
        //            "eventid INT PRIMARY KEY, " +
        //            "eventAdmin VARCHAR(50), " +
        //            "eventName VARCHAR(255), " +
        //            "eventInvite VARCHAR(255), " +
        //            "eventTeams INT, " +
        //            "eventPlayerCount INT, " +
        //            "eventTime INT, " +
        //            "eventActive VARCHAR(10) " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS EventSpawns (" +
        //            "eventid INT NOT NULL, " +
        //            "eventTeam INT NOT NULL, " +
        //            "eventSpawn VARCHAR(10), " +
        //            "eventRespawn VARCHAR(10), " +
        //            "FOREIGN KEY(eventid) REFERENCES Events(eventid) " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS Hardcore (" +
        //            "eventid INT PRIMARY KEY, " +
        //            "steamid VARCHAR(20) NOT NULL, " +
        //            "sessionTime INT DEFAULT 0, " +
        //            "kills INT DEFAULT 0, " +
        //            "zKills INT DEFAULT 0, " +
        //            "score INT DEFAULT 0, " +
        //            "deaths INT DEFAULT 0, " +
        //            "playerName VARCHAR(50) NOT NULL " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS Tracking (" +
        //            "trackid INT PRIMARY KEY, " +
        //            "dateTime VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM', " +
        //            "position VARCHAR(20) NOT NULL, " +
        //            "steamId VARCHAR(20) NOT NULL, " +
        //            "playerName VARCHAR(50) NOT NULL, " +
        //            "holding VARCHAR(100) NOT NULL " +
        //            ");", "MySqlDB");
        //        FastQuery("CREATE TABLE IF NOT EXISTS Config (sql_version INTEGER);", "MySqlDB");
        //        int _version = 1;
        //        DataTable _result = TypeQuery("SELECT sql_version FROM Config");
        //        if (_result.Rows.Count == 0)
        //        {
        //            FastQuery("INSERT INTO Config (sql_version) VALUES (1)", "MySqlDB");
        //        }
        //        else
        //        {
        //            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _version);
        //        }
        //        _result.Dispose();
        //        if (_version != SQL.Sql_version)
        //        {
        //            UpdateSQL.Exec(_version);
        //        }
        //        else
        //        {
        //            LoadProcess.Load(4);
        //        }
        //    }
        //    catch (MySqlException e)
        //    {
        //        Log.Out(string.Format("[ServerTools] MySqlException in MySqlDB.CreateTables: {0}", e));
        //    }
        //}
        //
        //public static void FastQuery(string _sql, string _class)
        //{
        //    try
        //    {
        //        connection.Open();
        //        cmd = new MySqlCommand(_sql, connection);
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (MySqlException e)
        //    {
        //        Log.Out(string.Format("[ServerTools] MySqlException in MySqlDB.FastQuery: {0}. Class source {1}", e, _class));
        //    }
        //    connection.Close();
        //}
        //
        //public static DataTable TypeQuery(string _sql)
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        connection.Open();
        //        cmd = new MySqlCommand(_sql, connection);
        //        MySqlDataReader _reader = cmd.ExecuteReader();
        //        dt.Load(_reader);
        //        _reader.Close();
        //    }
        //    catch (MySqlException e)
        //    {
        //        Log.Out(string.Format("[ServerTools] MySqlException in MySqlDB.TQuery: {0}", e));
        //    }
        //    connection.Close();
        //    return dt;
        //}
    }
}

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
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD return VARCHAR(50) DEFAULT 'false';");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD eventRespawn VARCHAR(50) DEFAULT 'false';");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD eventSpawn VARCHAR(50) DEFAULT 'false';");
                }
                else
                {
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD return TEXT DEFAULT 'false';", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD eventRespawn TEXT DEFAULT 'false';", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD eventSpawn TEXT DEFAULT 'false';", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 2 WHERE sql_version = 1", "UpdateSQL");
            }
            if (_version == 2)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns DROP eventSpawn;");
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns DROP eventRespawn;");
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns ADD eventSpawn VARCHAR(50) DEFAULT 'false';");
                    MySqlDatabase.FastQuery("ALTER TABLE EventSpawns ADD eventRespawn VARCHAR(50) DEFAULT 'false';");
                }
                else
                {
                    SQLiteDatabase.FastQuery("DROP TABLE EventSpawns", "UpdateSQL");
                    SQLiteDatabase.FastQuery("CREATE TABLE IF NOT EXISTS EventSpawns (eventid INTEGER NOT NULL, eventTeam INTEGER NOT NULL, eventSpawn TEXT, eventRespawn TEXT, FOREIGN KEY(eventid) REFERENCES Events(eventid))", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 3 WHERE sql_version = 2", "UpdateSQL");
            }
            if (_version == 3)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("ALTER TABLE Auction ADD sellDate VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM';");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD newSpawn VARCHAR(50) DEFAULT 'false'; ");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD messageCount INT DEFAULT 0;");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD messageTime VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM';");
                }
                else
                {
                    SQLiteDatabase.FastQuery("ALTER TABLE Auction ADD sellDate TEXT DEFAULT '10/29/2000 7:30:00 AM';", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD newSpawn TEXT DEFAULT 'false';", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD messageCount INTEGER DEFAULT 0;", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD messageTime TEXT DEFAULT '10/29/2000 7:30:00 AM';", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 4 WHERE sql_version = 3", "UpdateSQL");
            }
            if (_version == 4)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("CREATE TABLE IF NOT EXISTS Vehicles (steamid INTEGER PRIMARY KEY, bikeId INT DEFAULT 0, lastBike VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM', miniBikeId INT DEFAULT 0, lastMiniBike VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM', motorBikeId INT DEFAULT 0, lastMotorBike VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM', jeepId INT DEFAULT 0, lastJeep VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM', gyroId INT DEFAULT 0, lastGyro VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM')");
                }
                else
                {
                    SQLiteDatabase.FastQuery("CREATE TABLE IF NOT EXISTS Vehicles (steamid INTEGER PRIMARY KEY, bikeId INTEGER DEFAULT 0, lastBike TEXT DEFAULT '10/29/2000 7:30:00 AM', miniBikeId INTEGER DEFAULT 0, lastMiniBike TEXT DEFAULT '10/29/2000 7:30:00 AM', motorBikeId INTEGER DEFAULT 0, lastMotorBike TEXT DEFAULT '10/29/2000 7:30:00 AM', jeepId INTEGER DEFAULT 0, lastJeep TEXT DEFAULT '10/29/2000 7:30:00 AM', gyroId INTEGER DEFAULT 0, lastGyro TEXT DEFAULT '10/29/2000 7:30:00 AM')", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 5 WHERE sql_version = 4", "UpdateSQL");
            }
            if (_version == 5)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD lastVoteWeekly VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM';");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD weeklyVoteCount INT DEFAULT 0;");
                }
                else
                {
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD lastVoteWeekly TEXT DEFAULT '10/29/2000 7:30:00 AM';", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD weeklyVoteCount INTEGER DEFAULT 0;", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 6 WHERE sql_version = 5", "UpdateSQL");
            }
            if (_version == 6)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("CREATE TABLE IF NOT EXISTS Tracking (Id INTEGER PRIMARY KEY, dateTime VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM, position VARCHAR(50) NOT NULL, steamId VARCHAR(50) NOT NULL, playerName VARCHAR(50) NOT NULL, holding VARCHAR(50) NOT NULL)");
                }
                else
                {
                    SQLiteDatabase.FastQuery("CREATE TABLE IF NOT EXISTS Tracking (Id INTEGER PRIMARY KEY, dateTime TEXT DEFAULT '10/29/2000 7:30:00 AM', position TEXT NOT NULL, steamId TEXT NOT NULL, playerName TEXT NOT NULL, holding TEXT NOT NULL)", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 7 WHERE sql_version = 6", "UpdateSQL");
            }
            if (_version == 7)
            {
                if (SQL.IsMySql)
                {
                    MySqlDatabase.FastQuery("CREATE TABLE IF NOT EXISTS Hardcore (Id INTEGER PRIMARY KEY, steamid VARCHAR(50) NOT NULL, sessionTime INT DEFAULT 0, kills INT DEFAULT 0, zKills INT DEFAULT 0, score INT DEFAULT 0, deaths INT DEFAULT 0, playerName VARCHAR(50) NOT NULL)");
                    MySqlDatabase.FastQuery("ALTER TABLE Players ADD extraLives INT DEFAULT 0;");
                }
                else
                {
                    SQLiteDatabase.FastQuery("CREATE TABLE IF NOT EXISTS Hardcore (Id INTEGER PRIMARY KEY, steamid TEXT NOT NULL, sessionTime INTEGER DEFAULT 0, kills INTEGER DEFAULT 0, zKills INTEGER DEFAULT 0, score INTEGER DEFAULT 0, deaths INTEGER DEFAULT 0, playerName TEXT NOT NULL)", "UpdateSQL");
                    SQLiteDatabase.FastQuery("ALTER TABLE Players ADD extraLives INTEGER DEFAULT 0;", "UpdateSQL");
                }
                SQL.FastQuery("UPDATE Config SET sql_version = 8 WHERE sql_version = 7", "UpdateSQL");
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
            else
            {
                LoadProcess.Load(3);
            }
        }
    }
}

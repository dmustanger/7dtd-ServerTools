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
            FastQuery("CREATE TABLE IF NOT EXISTS Players (steamid TEXT PRIMARY KEY ON CONFLICT REPLACE, pingimmunity INTEGER, last_gimme TEXT);");
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

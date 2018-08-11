using MySql.Data.MySqlClient;
using System.Data;

namespace ServerTools
{
    public class MySqlDatabase
    {
        private static MySqlConnection connection;
        private static MySqlCommand cmd;
        private static string Server;
        private static string Database;
        private static string UserName;
        private static string Password;

        public static void SetConnection()
        {
            //remove after set in config
            Server = "localhost";
            Database = "connectcsharptomysql";
            UserName = "username";
            Password = "password";
            //---------------------------

            string _connectionString;
            _connectionString = string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};", Server, Database, UserName, Password);
            connection = new MySqlConnection(_connectionString);
            try
            {
                Log.Out("[ServerTools] Connecting to MySql Database.");
                connection.Open();
            }
            catch (MySqlException e)
            {
                switch (e.Number)
                {
                    case 0:
                        Log.Out("[ServerTools] MySqlException in MySqlDatabase.SetConnection: Cannot connect to server.");
                        break;

                    case 1045:
                        Log.Out("[ServerTools] MySqlException in MySqlDatabase.SetConnection: Invalid username/password, please try again.");
                        break;
                }
                return;
            }
            connection.Close();
            CreateTables();
        }

        private static void CreateTables()
        {
            FastQuery("CREATE TABLE IF NOT EXISTS Players (steamid VARCHAR(50) NOT NULL, playername VARCHAR(50) DEFAULT 'Unknown', last_joined VARCHAR(50) DEFAULT 'Never', pingimmunity VARCHAR(10) DEFAULT 'false', last_gimme VARCHAR(50) DEFAULT '10/29/2000 7:30:00 AM',PRIMARY KEY (steamid)) ENGINE = InnoDB;");
            FastQuery("CREATE TABLE IF NOT EXISTS Config (sql_version INTEGER) ENGINE = InnoDB;");
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

        public static void FastQuery(string _sql)
        {
            try
            {
                connection.Open();
                cmd = new MySqlCommand(_sql, connection);
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch (MySqlException e)
            {
                Log.Out(string.Format("[ServerTools] MySqlException in MySqlException.FastQuery: {0}", e));
            }
        }

        public static DataTable TQuery(string _sql)
        {
            DataTable dt = new DataTable();
            try
            {
                connection.Open();
                cmd = new MySqlCommand(_sql, connection);
                MySqlDataReader _reader = cmd.ExecuteReader();
                dt.Load(_reader);
                _reader.Close();
                connection.Close();
            }
            catch (MySqlException e)
            {
                Log.Out(string.Format("[ServerTools] MySqlException in MySqlException.TQuery: {0}", e));
            }
            return dt;
        }

        public static string EscapeString(string _string)
        {
            string _str = MySqlHelper.EscapeString(_string);
            return _str;
        }
    }
}

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
            FastQuery("CREATE TABLE IF NOT EXISTS Players (steamid VARCHAR(50) NOT NULL, pingimmunity VARCHAR(10) DEFAULT 'false', last_gimme DATE,PRIMARY KEY (steamid)) ENGINE = InnoDB;");
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

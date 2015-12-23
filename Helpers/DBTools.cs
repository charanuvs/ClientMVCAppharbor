using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMVC.Helpers
{
    class DBTools
    {
        private string server;
        private string database;
        private string uid;
        private string password;
        private MySql.Data.MySqlClient.MySqlConnection connection;

        public DBTools()
        {
            initilize();
        }

        public string GetName(string id)
        {
            string name = string.Empty;
            string query = "SELECT fullname FROM USERDETAILS WHERE id = " + "'" + id + "'";
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                {
                    name = dataReader["fullname"].ToString();
                }
                this.CloseConnection();
            }

            return name;
        }
        public bool Authenticate(string id, string password)
        {
            string dbPass = string.Empty;
            //connection.Open();
            string query = "SELECT pass FROM USERDETAILS WHERE id = " + "'" + id + "'";
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                {
                    dbPass = dataReader["pass"].ToString();
                    if (dbPass == password)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

                //close connection
                this.CloseConnection();
            }
            return false;
        }

        private void initilize()
        {
            server = "bravodbinstance.c7nqnuxewgdw.us-west-2.rds.amazonaws.com";
            database = "demo";
            uid = "root";
            password = "charan92";
            string connectionString;
            connectionString = "Server = " + server + "; Database = " + database + "; Uid = " + uid + "; Pwd = " + password;

            connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
        }
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                return false;
            }
        }
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                return false;
            }
        }
    }
}

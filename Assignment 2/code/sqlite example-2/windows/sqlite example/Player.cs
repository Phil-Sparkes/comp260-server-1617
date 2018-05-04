using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#if TARGET_LINUX
using Mono.Data.Sqlite;
using sqliteConnection 	=Mono.Data.Sqlite.SqliteConnection;
using sqliteCommand 	=Mono.Data.Sqlite.SqliteCommand;
using sqliteDataReader	=Mono.Data.Sqlite.SqliteDataReader;
#endif

#if TARGET_WINDOWS
using System.Data.SQLite;
using sqliteConnection = System.Data.SQLite.SQLiteConnection;
using sqliteCommand = System.Data.SQLite.SQLiteCommand;
using sqliteDataReader = System.Data.SQLite.SQLiteDataReader;

#endif
namespace Server
{
    public class Player
    {
        public String username;
        public String password = "";
        public Boolean correctPassword = false;
        public String currentRoom = "Room 0";
        public String items = "$banana";
                                        
        public void Init(sqliteConnection conn)
        {
            try
            {
                //sqliteConnection.CreateFile(dungeonDatabase);

                //conn = new sqliteConnection("Data Source=" + dungeonDatabase + ";Version=3;FailIfMissing=True");
                //conn.Open();


                sqliteCommand command;

                try
                {
                    var sql = "insert into " + "table_players" + " (username, password, currentRoom, items, connected) values ";
                    sql += "('" + username + "'";
                    sql += ",";
                    sql += "'" + password + "'";
                    sql += ",";
                    sql += "'" + currentRoom + "'";
                    sql += ",";
                    sql += "'" + items + "'";
                    sql += ",";
                    sql += "'" + 1 + "'";
                    sql += ")";

                    command = new sqliteCommand(sql, conn);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to add player" + ex);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Create DB failed: " + ex);
            }

        }
    }
}


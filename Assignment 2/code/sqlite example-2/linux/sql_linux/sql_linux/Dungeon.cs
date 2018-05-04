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
    public class Dungeon
    {
        sqliteConnection conn = null;
        Dictionary<String, Room> roomMap;

       // String currentRoom;

        public void Init(sqliteConnection conn)
        {
            roomMap = new Dictionary<string, Room>();
            {
                var room = new Room("Room 0", "You are standing in the entrance hall\nAll adventures start here");
                room.north = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 1", "You are in room 2");
                room.north = "Room 2";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 2", "You are in room 3");
                room.south = "Room 1";
                room.west = "Room 3";
                room.east = "Room 6";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 3", "You are in room 4");
                room.north = "Room 4";
                room.south = "Room 5";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 4", "You are in room 5");
                room.south = "Room 3";
                room.item = "tomahawk";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 5", "You are in room 6");
                room.north = "Room 3";
                room.east = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 6", "You are in room 7");
                room.north = "Room 8";
                room.east = "Room 7";
                room.west = "Room 2";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 7", "You are in room 8");
                room.west = "Room 6";
                room.item = "key";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 8", "You are in room 9");
                room.north = "Room 9";
                room.south = "Room 6";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 9", "You are in room 10\nThe room has a large locked door");
                room.south = "Room 8";
                room.usefulItem = "key";
                room.resultFromItem = "Room 10";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 10", "You have reached the dragons lair");
                room.enemy = "Dragon";
                room.west = "Room 9";
                room.usefulItem = "tomahawk";
                room.resultFromItem = "Room 11";
                roomMap.Add(room.name, room);
            }
            {
                var room = new Room("Room 11", "You have slain the dragon\nCongratulations you have won!");
                roomMap.Add(room.name, room);
            }
            try
            {

                sqliteCommand command;

                foreach (var room in roomMap)
                {
                    command = new sqliteCommand("select * from table_rooms where name == '" + room.Key + "'", conn);
                    var reader = command.ExecuteReader();

                    if (reader.HasRows == false)
                    {
                        try
                        {
                            var sql = "insert into " + "table_rooms" + " (name, desc, north, east, south, west, enemy, item, usefulItem, resultFromItem) values ";
                            sql += "('" + room.Key + "'";
                            sql += ",";
                            sql += "'" + room.Value.desc + "'";
                            sql += ",";
                            sql += "'" + room.Value.north + "'";
                            sql += ",";
                            sql += "'" + room.Value.east + "'";
                            sql += ",";
                            sql += "'" + room.Value.south + "'";
                            sql += ",";
                            sql += "'" + room.Value.west + "'";
                            sql += ",";
                            sql += "'" + room.Value.enemy + "'";
                            sql += ",";
                            sql += "'" + room.Value.item + "'";
                            sql += ",";
                            sql += "'" + room.Value.usefulItem + "'";
                            sql += ",";
                            sql += "'" + room.Value.resultFromItem + "'";
                            sql += ")";

                            command = new sqliteCommand(sql, conn);
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to add room" + ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create DB failed: " + ex);
            }


        }

        public String GiveInfo(Client client, sqliteConnection conn)
        {
            String CurrentRoom = "";
            String CurrentItems = "";

            String info = "";
            var command = new sqliteCommand();
            command = new sqliteCommand("select * from table_players where username == '" + client.username + "'", conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    CurrentRoom = "" + reader["currentRoom"];
                    CurrentItems = "" + reader["items"];


                }
           
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to read from DB " + ex);
                }
            }
            command = new sqliteCommand("select * from table_rooms where name == '" + CurrentRoom + "'", conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                try
                {
                    info += reader["desc"];

                    if (reader["item"] != null)
                    {
                        if (!CurrentItems.Contains("" + reader["item"]))
                        {
                            info += "\nYou find a " + reader["item"] + " in the room";
                            CurrentItems += "$" + reader["item"];
                            var sql = "update " + "table_players" + " set items ='" + CurrentItems + "' where username = '" + client.username + "'";
                            command = new sqliteCommand(sql, conn);
                            command.ExecuteNonQuery();
                        }
                    }

                    if ("" + reader["enemy"] != "")
                    {
                        info += "\nYou find a fearsome " + reader["enemy"] + " blocking your way";
                    }

                    info += "\nExits\n";

                    if ("" + reader["north"] != "")
                    {
                        info += "North ";
                    }
                    if ("" + reader["east"] != "")
                    {
                        info += "East ";
                    }
                    if ("" + reader["south"] != "")
                    {
                        info += "South ";
                    }
                    if ("" + reader["west"] != "")
                    {
                        info += "West ";
                    }
                }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read from DB " + ex);
            }


            }
            info += "\nPlayers in room: ";
            command = new sqliteCommand("select * from table_players where currentRoom == '" + CurrentRoom + "'", conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                info += "[" + reader["username"] + "]";
            }
            info += "\n";
            return info;

        }

        public String Process(string Key, Client client, sqliteConnection conn)
        {
            String returnString = "";

            String usefulItem = "";
            String resultFromItem = "";
            String northRoom = "";
            String eastRoom = "";
            String southRoom = "";
            String westRoom = "";

            var command = new sqliteCommand();

            command = new sqliteCommand("select * from table_players where username == '" + client.username + "'", conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
                try
                {
                    {
                        client.currentRoom = "" + reader["currentRoom"];
                        client.playerItems = ("" + reader["items"]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to read from DB " + ex);
                }

            var playerItemsList = client.playerItems.Split('$');
            client.playerItemsList.Clear();
            foreach (var item in playerItemsList)
            {
                client.playerItemsList.Add("" + item);
            }
            command = new sqliteCommand("select * from table_rooms where name == '" + client.currentRoom + "'", conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
               usefulItem = "" + reader["usefulItem"];
               resultFromItem = "" + reader["resultFromItem"];
               northRoom= "" + reader["north"];
               eastRoom = "" + reader["east"];
               southRoom = "" + reader["south"];
               westRoom = "" + reader["west"];
            }


            var input = Key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    returnString += GiveInfo(client, conn);

                    returnString += "\nCommands are ....\n";
                    returnString += "help - for this screen\n";
                    returnString += "items - to check current items\n";
                    returnString += "use 'item' - uses an item\n";
                    returnString += "give 'playerName' 'item' - gives player item\n";
                    returnString += "say - local chat\n";
                    returnString += "global - global chat\n";
                    returnString += "name - change name\n";
                    returnString += "go [north | south | east | west]  - to travel between locations\n";
                    returnString += "\nPress any key to continue\n";

                    return returnString;

                case "items":
                    returnString += "Items: ";
                    foreach (string item in client.playerItemsList)
                    {
                        if (item != "")
                        {
                            returnString += "[" + item + "]";
                        }
                    }
                    return returnString;

                case "use":
                    String itemUse = input[0];
                    if (input.Length > 1)
                    {
                        itemUse = "";
                        for (var i = 1; i < input.Length; i++)
                        {
                            itemUse += (input[i] + "");
                        }
                    }
                    if (client.playerItemsList.Contains(itemUse))
                    {
                        if ((usefulItem != null) && (usefulItem == itemUse) && (client.playerItemsList.Contains(usefulItem)))
                        {
                            returnString += "used " + itemUse + "\n\n";
                            client.playerItemsList.Remove(itemUse);
                            String itemsUpdateDatabase = "";
                            foreach (String item in client.playerItemsList)
                            {
                                itemsUpdateDatabase += "$" + item;
                            }
                            client.currentRoom = resultFromItem;
                            command = new sqliteCommand("update " + "table_players" + " set currentRoom ='" + client.currentRoom + "' where username = '" + client.username + "'", conn);
                            command.ExecuteNonQuery();
                            command = new sqliteCommand("update " + "table_players" + " set items ='" + itemsUpdateDatabase + "' where username = '" + client.username + "'", conn);
                            command.ExecuteNonQuery();

                            returnString += GiveInfo(client, conn);
                        }
                        else
                        {
                            returnString += "cannot use " + itemUse;
                        }
                    }
                    else
                    {
                        returnString += "do not have item " + itemUse;
                    }
                    return returnString;

                case "give":
                    {
                        if (input.Length > 2)
                        {
                            String otherPlayer = input[1];
                            String tradeItem = input[2];
                            if (client.playerItemsList.Contains(tradeItem))
                            {
                                if (server.TradeItem(client, otherPlayer, tradeItem))
                                {
                                    returnString += ("[LOCAL][");
                                    returnString += (client.username);
                                    returnString += ("]");
                                    returnString += " has given [";
                                    returnString += (otherPlayer);
                                    returnString += ("]");
                                    returnString += (" a [");
                                    returnString += (tradeItem);
                                    returnString += ("]");
                                    return returnString;
                                }
                            }
                        }

                        returnString += ("cannot do that");
                        return returnString;
                    }

                case "say":
                    returnString += ("[LOCAL][");
                    returnString += (client.username);
                    returnString += ("] ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }
                    return returnString;

                case "global":
                    returnString += ("[GLOBAL][");
                    returnString += (client.username);
                    returnString += ("] ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }
                    return returnString;

                case "go":
                    String direction = "";

                    if (input.Length > 1)
                    {
                        direction = input[1].ToLower();
                    }

                    if ((direction == "north") && (northRoom != ""))
                    {
                        client.currentRoom = northRoom;
                    }
                    else if ((direction == "south") && (southRoom != ""))
                    {
                        client.currentRoom = southRoom;
                    }
                    else if ((direction == "east") && (eastRoom != ""))
                    {
                        client.currentRoom = eastRoom;
                    }
                    else if ((direction == "west") && (westRoom != ""))
                    {
                        client.currentRoom = westRoom;
                    }
                    else
                    {

                        returnString += GiveInfo(client, conn);
                        returnString += "\nERROR";
                        returnString += "\nCan not go " + direction + " from here\n";
                    }

                    var sql = "update " + "table_players" + " set currentRoom ='" + client.currentRoom + "' where username = '" + client.username + "'";
                    command = new sqliteCommand(sql, conn);
                    command.ExecuteNonQuery();

                    returnString += GiveInfo(client, conn);
                    return returnString;

                default:
                    //handle error
                    returnString += "\nERROR";
                    returnString += "\nCan not " + Key;
                    return returnString;
            }

        }
    }
}

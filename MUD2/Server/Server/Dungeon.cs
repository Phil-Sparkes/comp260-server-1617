using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    public class Dungeon
    {
        public Dictionary<String, Room> roomMap;

        Room currentRoom;

        public void Init()
        {
            roomMap = new Dictionary<string, Room>();
            {
                var room = new Room("Room 0", "You are standing in the entrance hall\nAll adventures start here");
                room.north = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 1", "You are in room 1");
                room.south = "Room 0";
                room.west = "Room 3";
                room.east = "Room 2";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 2", "You are in room 2");
                room.north = "Room 4";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 3", "You are in room 3");
                room.east = "Room 1";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 4", "You are in room 4");
                room.south = "Room 2";
                room.west = "Room 5";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 5", "You are in room 5");
                room.south = "Room 1";
                room.east = "Room 4";
                roomMap.Add(room.name, room);
            }

            currentRoom = roomMap["Room 0"];
        }

        public String GiveInfo(Player player)
        {
            currentRoom = player.currentRoom;
            String info = "";
            info += currentRoom.desc;
            info += "\nExits\n";
            for (var i = 0; i < currentRoom.exits.Length; i++)
            {
                if (currentRoom.exits[i] != null)
                {
                    info += (Room.exitNames[i] + " ");
                }
            }
            return info;

        }
        public String Process(string Key, Player player)
        {
            currentRoom = player.currentRoom;
            //var key = Console.ReadLine();
            String returnString = ""; GiveInfo(player);
            var input = Key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    returnString += GiveInfo(player);

                    returnString += "\nCommands are ....\n";
                    returnString += "help - for this screen\n";
                    returnString += "look - to look around\n";
                    returnString += "go [north | south | east | west]  - to travel between locations\n";
                    returnString += "\nPress any key to continue\n";
                    
                    return returnString;

                case "look":
                    //loop straight back
                    Console.Clear();
                    Thread.Sleep(1000);
                    returnString += GiveInfo(player);
                    return returnString;

                case "say":
                    returnString += GiveInfo(player);
                    returnString += ("\nYou say ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }

                    return returnString;

                case "go":
                    // is arg[1] sensible?
                    if ((input[1].ToLower() == "north") && (currentRoom.north != null))
                    {
                        player.currentRoom = roomMap[currentRoom.north];
                    }
                    else
                    {
                        if ((input[1].ToLower() == "south") && (currentRoom.south != null))
                        {
                            player.currentRoom = roomMap[currentRoom.south];
                        }
                        else
                        {
                            if ((input[1].ToLower() == "east") && (currentRoom.east != null))
                            {
                                player.currentRoom = roomMap[currentRoom.east];
                            }
                            else
                            {
                                if ((input[1].ToLower() == "west") && (currentRoom.west != null))
                                {
                                    player.currentRoom = roomMap[currentRoom.west];
                                }
                                else
                                {
                                    //handle error
                                    returnString += GiveInfo(player);
                                    returnString += "\nERROR";
                                    returnString += "\nCan not go " + input[1] + " from here";
                                }
                            }
                        }
                    }
                    returnString += GiveInfo(player);
                    return returnString;

                default:
                    //handle error
                    returnString += GiveInfo(player);
                    returnString += "\nERROR";
                    returnString += "\nCan not " + Key;
                    return returnString;
            }

        }
    }
}

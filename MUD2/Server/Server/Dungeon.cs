using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    public class Dungeon
    {
        Dictionary<String, Room> roomMap;

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

        public String Options()
        {
            String MsgToSend = "";
            MsgToSend += currentRoom.desc;
            MsgToSend += "\nExits\n";
            for (var i = 0; i < currentRoom.exits.Length; i++)
            {
                if (currentRoom.exits[i] != null)
                {
                    MsgToSend += (Room.exitNames[i] + " ");
                }
            }
            return MsgToSend;

        }
        public String Process(string message)
        {
            //var key = Console.ReadLine();
            var key = message;
            String MsgToSend = "";
            var input = key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    Console.Clear();
                    MsgToSend += "\nCommands are ..../n";
                    MsgToSend += "help - for this screen/n";
                    MsgToSend += "look - to look around/n";
                    MsgToSend += "go [north | south | east | west]  - to travel between locations/n";
                    MsgToSend += "\nPress any key to continue/n";

                    return MsgToSend;

                case "look":
                    //loop straight back
                    Console.Clear();
                    Thread.Sleep(1000);
                    return MsgToSend;

                case "say":
                    Console.Write("You say ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        Console.Write(input[i] + " ");
                    }

                    Thread.Sleep(1000);
                    Console.Clear();
                    return MsgToSend;

                case "go":
                    // is arg[1] sensible?
                    if ((input[1].ToLower() == "north") && (currentRoom.north != null))
                    {
                        currentRoom = roomMap[currentRoom.north];
                    }
                    else
                    {
                        if ((input[1].ToLower() == "south") && (currentRoom.south != null))
                        {
                            currentRoom = roomMap[currentRoom.south];
                        }
                        else
                        {
                            if ((input[1].ToLower() == "east") && (currentRoom.east != null))
                            {
                                currentRoom = roomMap[currentRoom.east];
                            }
                            else
                            {
                                if ((input[1].ToLower() == "west") && (currentRoom.west != null))
                                {
                                    currentRoom = roomMap[currentRoom.west];
                                }
                                else
                                {
                                    //handle error
                                    MsgToSend += "\nERROR";
                                    MsgToSend += "\nCan not go " + input[1] + " from here";
                                    MsgToSend += "\nPress any key to continue";
                                }
                            }
                        }
                    }
                    return MsgToSend;

                default:
                    //handle error
                    MsgToSend += "\nERROR";
                    MsgToSend += "\nCan not " + key;
                    MsgToSend += "\nPress any key to continue\n";
                    return MsgToSend;
            }

        }
    }
}

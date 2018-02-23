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
                room.item = "Tomahawk";
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
                room.UsefulItem = "key";
                room.ResultfromItem = "Room 10";
                roomMap.Add(room.name, room);
            }

            {
                var room = new Room("Room 10", "You have reached the dragons lair");
                room.enemy = "Dragon";
                room.west = "Room 9";
                room.UsefulItem = "Tomahawk";
                room.ResultfromItem = "Room 11";
                roomMap.Add(room.name, room);
            }
            { 
                var room = new Room("Room 11", "You have slain the dragon\nCongratulations you have won!");
                roomMap.Add(room.name, room);
            }
        currentRoom = roomMap["Room 0"];
        }

        public String GiveInfo(Player player)
        {
            currentRoom = player.currentRoom;
            String info = "";
            info += currentRoom.desc;

            if (currentRoom.item != null)
                {
                if (!player.Items.Contains(currentRoom.item))
                {
                    info += "\nYou find a " + currentRoom.item + " in the room";
                    player.Items.Add(currentRoom.item);
                }
            }

            if (currentRoom.enemy != null)
            {
                info += "\nYou find a fearsome " + currentRoom.enemy + " blocking your way";
            }

            info += "\nExits\n";

            for (var i = 0; i < currentRoom.exits.Length; i++)
            {
                if (currentRoom.exits[i] != null)
                {
                    info += (Room.exitNames[i] + " ");
                }
            }

            info += "\nPlayers in room: ";

            foreach (Player otherPlayer in server.PlayerList)
            {
                if (player.currentRoom == otherPlayer.currentRoom)
                {
                    info += "[";
                    info += otherPlayer.playerName;
                    info += "]";
                }
            }
            info += "\n";
            return info;

        }
        public String Process(string Key, Player player)
        {
            currentRoom = player.currentRoom;
            String returnString = ""; GiveInfo(player);
            var input = Key.Split(' ');

            switch (input[0].ToLower())
            {
                case "help":
                    returnString += GiveInfo(player);

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
                    foreach (String item in player.Items)
                    {
                        returnString += "[" + item + "]";
                    }
                    return returnString;

                case "use":
                    String playeritem = " ";
                    if (input.Length > 1)
                    {
                        playeritem = "";
                        for (var i = 1; i < input.Length; i++)
                        {
                            playeritem += (input[i] + "");
                        }
                    }
                    if (player.Items.Contains(playeritem))
                    {
                        if ((currentRoom.UsefulItem != null) && (currentRoom.UsefulItem == playeritem) && (player.Items.Contains(currentRoom.UsefulItem)))
                        {
                            returnString += "used " + playeritem + "\n\n";
                            player.Items.Remove(playeritem);
                            player.currentRoom = roomMap[currentRoom.ResultfromItem];
                            returnString += GiveInfo(player);
                        }
                        else
                        {
                            returnString += "cannot use " + playeritem;
                        }
                    }
                    else
                    {
                        returnString += "do not have item " + playeritem;
                    }
                    return returnString;

                case "give":
                    {
                        if (input.Length > 2)
                        {
                            String otherPlayer = input[1];
                            String tradeItem = input[2];
                            if (player.Items.Contains(tradeItem))
                            { 
                                if (server.TradeItem(player, otherPlayer, tradeItem))
                                {
                                    returnString += ("[LOCAL][");
                                    returnString += (player.playerName);
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
                    returnString += (player.playerName);
                    returnString += ("] ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }

                    return returnString;
                case "global":
                    returnString += ("[GLOBAL][");
                    returnString += (player.playerName);
                    returnString += ("] ");
                    for (var i = 1; i < input.Length; i++)
                    {
                        returnString += (input[i] + " ");
                    }

                    return returnString;

                case "name":
                    String newName = " ";
                    if (input.Length > 1)
                    {
                        newName = input[1];
                    }
                    returnString += ("[SERVER][");
                    returnString += (player.playerName);
                    returnString += ("] has changed their name to [");
                    returnString += (newName);
                    returnString += ("] ");
                    player.playerName = newName;
                   
                    return returnString;

                case "go":
                    String direction = "";

                    if (input.Length > 1)
                    {
                        direction = input[1].ToLower();
                    }

                    if ((direction == "north") && (currentRoom.north != null))
                    {
                        player.currentRoom = roomMap[currentRoom.north];
                        returnString += GiveInfo(player);
                    }
                    else
                    {
                        if ((direction == "south") && (currentRoom.south != null))
                        {
                            player.currentRoom = roomMap[currentRoom.south];
                            returnString += GiveInfo(player);
                        }
                        else
                        {
                            if ((direction == "east") && (currentRoom.east != null))
                            {
                                player.currentRoom = roomMap[currentRoom.east];
                                returnString += GiveInfo(player);
                            }
                            else
                            {
                                if ((direction == "west") && (currentRoom.west != null))
                                {
                                    player.currentRoom = roomMap[currentRoom.west];
                                    returnString += GiveInfo(player);
                                }
                                else
                                {

                                    returnString += GiveInfo(player);
                                    returnString += "\nERROR";
                                    returnString += "\nCan not go " + direction  + " from here\n";
                                }
                            }
                        }
                    }

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

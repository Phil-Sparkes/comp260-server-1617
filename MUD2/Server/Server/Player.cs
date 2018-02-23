using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Server
{
    public class Player
    {
        public Dungeon dungeonRef;
        public Room currentRoom;
        public String playerName;
        public String clientName;
        public List<String> Items = new List<string>();
        public void Init()
        {
           currentRoom = dungeonRef.roomMap["Room 0"];
           Items.Add("banana");
        }
    }
}

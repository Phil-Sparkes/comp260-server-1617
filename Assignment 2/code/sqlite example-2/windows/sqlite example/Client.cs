using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Client
    {
        public Boolean inputUsername = false;
        public Boolean inputPassword = false;
        public String clientName;


        public String username;
        public String currentRoom;
        public String playerItems;
        public List<string> playerItemsList = new List<string>();
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Room
    {
        public Room(String name, String desc)
        {
            this.desc = desc;
            this.name = name;
            item = null;
            enemy = null;
            usefulItem = null;
            resultFromItem = null;
        }

        public String north
        {
            get { return exits[0]; }
            set { exits[0] = value; }
        }

        public String east
        {
            get { return exits[1]; }
            set { exits[1] = value; }
        }

        public String south
        {
            get { return exits[2]; }
            set { exits[2] = value; }
        }
        public String west
        {
            get { return exits[3]; }
            set { exits[3] = value; }
        }


        public String enemy = "";
        public String item = "";
        public String name = "";
        public String desc = "";
        public String usefulItem = "";
        public String resultFromItem = "";
        public String[] exits = new String[4];
        public static String[] exitNames = { "NORTH", "EAST", "SOUTH", "WEST" };
    }

}

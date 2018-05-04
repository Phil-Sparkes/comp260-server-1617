using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
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
    class server
    {
        static bool quit = false;

        public static string dungeonDatabase = "data.database";
        public static sqliteConnection conn = new sqliteConnection("Data Source=" + dungeonDatabase + ";Version=3;FailIfMissing=True");


        static LinkedList<String> incommingMessages = new LinkedList<string>();
        static LinkedList<String> outgoingMessages = new LinkedList<string>();

        public static List<Client> clientList = new List<Client>();

        static Dictionary<String, Socket> clientDictionary = new Dictionary<String, Socket>();

        static Dungeon dungeon = new Dungeon();

        class ReceiveThreadLaunchInfo
        {
            public ReceiveThreadLaunchInfo(int ID, Socket socket)
            {
                this.ID = ID;
                this.socket = socket;
            }

            public int ID;
            public Socket socket;

        }

        static void acceptClientThread(Object obj)
        {
            Socket s = obj as Socket;

            int ID = 1;

            while (quit == false)
            {
                var newClientSocket = s.Accept();

                var myThread = new Thread(clientReceiveThread);
                myThread.Start(new ReceiveThreadLaunchInfo(ID, newClientSocket));


                lock (clientDictionary)
                {

                    String clientName = "client" + ID;
                    clientDictionary.Add(clientName, newClientSocket);
                    var client = new Client
                    {
                        clientName = clientName
                    };
                    clientList.Add(client);

                    lock (outgoingMessages)
                    {
                        outgoingMessages.AddLast(clientName + "¬" + "please enter username:");
                    }
                    ID++;
                }
            }
        }

        static Socket GetSocketFromName(String name)
        {
            lock (clientDictionary)
            {
                return clientDictionary[name];
            }
        }

        static String GetNameFromSocket(Socket s)
        {
            lock (clientDictionary)
            {
                foreach (KeyValuePair<String, Socket> o in clientDictionary)
                {
                    if (o.Value == s)
                    {
                        return o.Key;
                    }
                }
            }
            return null;
        }

        public static bool TradeItem(Client ClientGive, String ClientReceive, String Item)
        {
            foreach (Client client in clientList)
            {
                if (client.username == ClientReceive)
                {
                    Client otherClient = client;
                    if (otherClient.currentRoom == ClientGive.currentRoom)
                    {
                        otherClient.playerItemsList.Add(Item);
                        ClientGive.playerItemsList.Remove(Item);

                        String itemsUpdateDatabase = "";
                        foreach (String item in ClientGive.playerItemsList)
                        {
                            itemsUpdateDatabase += "$" + item;
                        }
                        Console.WriteLine(itemsUpdateDatabase);
                        var command = new sqliteCommand("update " + "table_players" + " set items ='" + itemsUpdateDatabase + "' where username = '" + ClientGive.username + "'", conn);
                        command.ExecuteNonQuery();

                        itemsUpdateDatabase = "";
                        foreach (String item in otherClient.playerItemsList)
                        {
                            itemsUpdateDatabase += "$" + item;
                        }
                        Console.WriteLine(itemsUpdateDatabase);
                        command = new sqliteCommand("update " + "table_players" + " set items ='" + itemsUpdateDatabase + "' where username = '" + otherClient.username + "'", conn);
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            return false;
        }



        static void localChatMessage(string message, Client client)
        {
            foreach (Client otherClient in clientList)
            {
                if (client.currentRoom == otherClient.currentRoom)
                {
                    lock (outgoingMessages)
                    {
                        outgoingMessages.AddLast(otherClient.clientName + "¬" + message);
                    }
                }

            }

        }

        static void globalChatMessage(string message)
        {
            lock (outgoingMessages)
            {
                foreach (KeyValuePair<String, Socket> test in clientDictionary)
                {
                    outgoingMessages.AddLast(test.Key + "¬" + message);
                }
            }
        }

        static void clientReceiveThread(Object obj)
        {
            ReceiveThreadLaunchInfo receiveInfo = obj as ReceiveThreadLaunchInfo;
            bool socketLost = false;

            while ((quit == false) && (socketLost == false))
            {
                byte[] buffer = new byte[4094];

                try
                {
                    int result = receiveInfo.socket.Receive(buffer);

                    if (result > 0)
                    {
                        ASCIIEncoding encoder = new ASCIIEncoding();

                        lock (incommingMessages)
                        {
                            incommingMessages.AddLast(receiveInfo.ID + "¬" + encoder.GetString(buffer, 0, result));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    socketLost = true;
                }
            }
        }

        static bool checkUserExists(string username)
        {
            try
            {

                var command = new sqliteCommand("select * from table_players where username == '" + username + "'", conn);
                var reader = command.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read from DB " + ex);
                return false;
            }
        }
        static void createNewUser(string username)
        {
            
            var player = new Player
            {
                username = username
            };
            player.Init(conn);
            Console.WriteLine("user created");
        }

        static void checkPassword(Client client, string password)
        {
            var command = new sqliteCommand("select * from table_players where username == '" + client.username + "'", conn);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                try
                {
                    if ("" + reader["password"] == password)
                    {
                        Console.WriteLine("correct password");
                        client.inputPassword = true;
                        outgoingMessages.AddLast(client.clientName + "¬" + dungeon.GiveInfo(client, conn));
                    }
                    else if ("" + reader["password"] == "")
                    {
                        Console.WriteLine("password created");
                        var sql = "update " + "table_players" + " set password ='" + password + "' where username = '" + client.username + "'";
                        command = new sqliteCommand(sql, conn);
                        command.ExecuteNonQuery();
                        client.inputPassword = true;
                        outgoingMessages.AddLast(client.clientName + "¬" + dungeon.GiveInfo(client, conn));
                    }
                    else
                    {
                        Console.WriteLine("incorrect password");
                        outgoingMessages.AddLast(client.clientName + "¬" + "incorrect password");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to read from DB " + ex);
                }
            }
        }
        static void Main(string[] args)
        {
            //sqliteConnection.CreateFile(dungeonDatabase);
            conn.Open();

            sqliteCommand command;

            command = new sqliteCommand("create table if not exists table_rooms (name varchar(7), desc varchar(40), north varchar(7), east varchar(7), south varchar(7), west varchar(7), enemy varchar(10), item varchar(8), usefulItem varchar(10), resultFromItem varchar(7))", conn);
            command.ExecuteNonQuery();
            command = new sqliteCommand("create table if not exists table_players (username varchar(10), password varchar(10), currentRoom varchar(7), items varchar(20), connected int)", conn);
            command.ExecuteNonQuery();

            ASCIIEncoding encoder = new ASCIIEncoding();

            dungeon.Init(conn);

            Socket serverClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

            serverClient.Bind(ipLocal);
            serverClient.Listen(4);

            Console.WriteLine("Waiting for client ...");

            var myThread = new Thread(acceptClientThread);
            myThread.Start(serverClient);


            byte[] buffer = new byte[4096];

            while (true)
            {
                String messageToSend = "";

                lock (outgoingMessages)
                {
                    if (outgoingMessages.First != null)
                    {

                        messageToSend = outgoingMessages.First.Value;

                        outgoingMessages.RemoveFirst();
                    }
                }

                if (messageToSend != "")
                {

                    try
                    {

                        String[] substrings = messageToSend.Split('¬');

                        string theClient = substrings[0];
                        string dungeonResult = substrings[1];

                        byte[] sendBuffer = encoder.GetBytes(dungeonResult); // this is sending back to client
                        //byte[] sendBuffer = new byte[4096];
                        int bytesSent = GetSocketFromName(theClient).Send(sendBuffer);

                        bytesSent = GetSocketFromName(theClient).Send(sendBuffer); // DO NOT KNOW WHY I HAVE TO DO THIS TWICE BUT ONLY WAY IT WORKS
                        Console.WriteLine("sending message to " + theClient);
                    }
                    catch
                    {

                    }
                }

                String labelToPrint = "";
                lock (incommingMessages)
                {
                    if (incommingMessages.First != null)
                    {
                        labelToPrint = incommingMessages.First.Value;

                        incommingMessages.RemoveFirst();

                    }
                }

                if (labelToPrint != "")
                {

                    String[] substrings = labelToPrint.Split('¬');

                    int ClientID = Int32.Parse(substrings[0]) - 1;
                    String clientMessage = substrings[1];
                    
                    var theClient = clientList[ClientID];

                    if (theClient.inputUsername == false)
                    {
                        theClient.username = clientMessage;
                        if (checkUserExists(theClient.username))
                        {
                            Console.WriteLine("user exists");
                            outgoingMessages.AddLast(theClient.clientName + "¬" + "please input the password for user: " + theClient.username);
                            theClient.inputUsername = true;
                        }
                        else {
                            Console.WriteLine("new user");
                            outgoingMessages.AddLast(theClient.clientName + "¬" + "please create a password for user: " + theClient.username);
                            createNewUser(theClient.username);
                            theClient.inputUsername = true;
                        }
                    }
                    else if (theClient.inputPassword == false)
                    {
                        checkPassword(theClient, clientMessage);
                    }
                    else
                    {
                        var dungeonResult = dungeon.Process(clientMessage, theClient, conn);
                        Console.WriteLine(dungeonResult);

                        if (dungeonResult.Length > 7)
                        {
                            if (dungeonResult.Substring(0, 7) == "[LOCAL]")
                            {
                                localChatMessage(dungeonResult, theClient);
                            }
                            else if (dungeonResult.Substring(0, 8) == "[GLOBAL]" || dungeonResult.Substring(0, 8) == "[SERVER]")
                            {
                                globalChatMessage(dungeonResult);
                            }
                            else
                            {
                                lock (outgoingMessages)
                                {
                                    outgoingMessages.AddLast(theClient.clientName + "¬" + dungeonResult);
                                }
                            }
                        }
                        else
                        {
                            lock (outgoingMessages)
                            {
                                outgoingMessages.AddLast(theClient.clientName + "¬" + dungeonResult);
                            }
                        }

                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}

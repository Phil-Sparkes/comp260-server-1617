using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace Server
{
    class server
    {
        static void Main(string[] args)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();

            var dungeon = new Dungeon(); // make the dungeon

            dungeon.Init();

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

            s.Bind(ipLocal);
            s.Listen(4);

            Console.WriteLine("Waiting for client ...");

            Socket newConnection = s.Accept();
            var dungeonResult = (dungeon.GiveInfo());
            byte[] sendBuffer = encoder.GetBytes(dungeonResult); // this is sending back to client

            int bytesSent = newConnection.Send(sendBuffer);

           
            if (newConnection != null)
            {            
                while (true)
                {
                    byte[] buffer = new byte[4096];

                    try
                    {
                        int result = newConnection.Receive(buffer);

                        if (result > 0)
                        {


                            String recdMsg = encoder.GetString(buffer, 0, result);
                            Console.WriteLine("Received: " + recdMsg);

                            dungeonResult = dungeon.Process(recdMsg);
                           

                            sendBuffer = encoder.GetBytes(dungeonResult); // this is sending back to client

                            bytesSent = newConnection.Send(sendBuffer);

                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);    	
                    }    
                    
                }
            }
        }
    }
}

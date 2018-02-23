using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class client
    {
        static LinkedList<String> incommingMessages = new LinkedList<string>();

        static void ReceiveMessages(Object obj)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] receiveBuffer = new byte[8192];

            Socket s = obj as Socket;

            while (true)
            {
                try
                {
                    int reciever = s.Receive(receiveBuffer);
                    s.Receive(receiveBuffer);
                    if (reciever > 0)
                    {
                        String userCmd = encoder.GetString(receiveBuffer, 0, reciever);
                        Console.WriteLine(userCmd);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

			bool connected = false;

			while (connected == false) 
			{
				try 
				{
                    s.Connect (ipLocal);
					connected = true;
				} 
				catch (Exception) 
				{
					Thread.Sleep (1000);
				}
			}

            int ID = 0;

            var myThread = new Thread(ReceiveMessages);
            myThread.Start(s);

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = new byte[4096];

            while (true)
            {
             
                //Console.Clear();
                String Msg = Console.ReadLine();
                String lowerMsg = Msg.ToLower() +  "      " ;
                if (lowerMsg.Substring(0, 3) != "say" && lowerMsg.Substring(0, 6) != "global" && lowerMsg.Substring(0, 4) != "name")
                {
                    Console.Clear();
                }
                else
                {
                    ClearLine();
                }
             
                ID++;
                
                buffer = encoder.GetBytes(Msg);

                try
                {
                    //Console.WriteLine("Writing to server: " + Msg);
                    int bytesSent = s.Send(buffer);


                   // buffer = new byte[4096];
                    //int receiver = s.Receive(buffer);
                    //s.Receive(buffer);
                    //if (receiver > 0)
                    //{
                    //    String userCmd = encoder.GetString(buffer, 0, receiver);
                    //    Console.WriteLine(userCmd);
                    //}


                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);	
                }

                //Thread.Sleep(1000);
            }
        }
    }
}

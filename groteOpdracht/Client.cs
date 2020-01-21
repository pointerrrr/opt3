using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace groteOpdracht
{
    class Client
    {
        public Client(string ip, int poort)
        {
            Run(ip, poort);
        }


        static NetworkStream stream;
        /// <summary>
        /// Runs the client, connects to a server and awaits input from this server
        /// </summary>
        /// <param name="server">Ip adress of the server</param>
        /// <param name="port">Port the server uses</param>
        static void Run(string server, int port)
        {
            try
            {
                //Setup server
                Console.Title ="client";
                var client = new TcpClient(server, port);
                stream = client.GetStream();
                var data = Encoding.ASCII.GetBytes("");
                var ls = new LocalSearch();
                //Read input
                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        data = new byte[256];
                        string response;
                        string[] responseSplit;
                        int bytes = stream.Read(data, 0, data.Length);
                        response = Encoding.ASCII.GetString(data, 0, bytes);
                        Console.WriteLine(response);
                        
                        //Start with empty solutio
                        if (response == "Start")
                        {
                            ls.currentSolution = new Solution();
                            ls.iterate();
                            sendMessage(ls.bestSolution.Value.ToString());
                        }
                        //Send current best solution
                        else if(response == "Solution")
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                int counter = 1;
                                for (int j = 0; j < 2; j++)
                                {

                                    var trip = ls.bestSolution.Truck1.Days[i, j];
                                    for (int x = 0; x < trip.Stops.Count; x++)
                                    {
                                        sendMessage(1 + ";" + (i + 1) + ";" + (counter++) + ";" + trip.Stops[x] + "\n");
                                    }
                                    sendMessage(1 + ";" + (i + 1) + ";" + (counter++) + ";" + 0 + "\n");
                                }
                                counter = 1;
                                for (int j = 0; j < 2; j++)
                                {
                                    var trip = ls.bestSolution.Truck2.Days[i, j];
                                    for (int x = 0; x < trip.Stops.Count; x++)
                                    {
                                        sendMessage(2 + ";" + (i + 1) + ";" + (counter++) + ";" + trip.Stops[x] + "\n");
                                    }
                                    sendMessage(2 + ";" + (i + 1) + ";" + (counter++) + ";" + 0 + "\n");

                                }
                            }
                            Thread.Sleep(500);
                            sendMessage("END");

                        }
                        //Start iteration with given parameters
                        else
                        {
                            responseSplit = response.Split(';');
                            Console.WriteLine(responseSplit.Length);
                            if (responseSplit.Length != 11)
                                continue;
                            ls.iterate(responseSplit);
                            sendMessage(ls.bestSolution.Value.ToString());
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("No connection possible");
                Console.WriteLine("Exception: {0}", e);
            }
        }

        /// <summary>
        /// Send message to the server
        /// </summary>
        /// <param name="message">Message that needs to send</param>
        private static void sendMessage(String message)
        {
            var data = System.Text.Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}

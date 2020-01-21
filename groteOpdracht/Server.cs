using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace groteOpdracht
{
    class Server
    {
        Random rng = new Random();
        TcpListener server = null;
        public bool serverstart = true;
        object lockObject;
        List<bool> start, finished, getSolution;
        List<double> results;
        List<int[]> inputs;
        List<Solution> solutions;
        string best;
        int counter = 0;
        public Server(string ip, int port)
        {
            var localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            lockObject = new object();
            start = new List<bool>();
            finished = new List<bool>();
            getSolution = new List<bool>();
            results = new List<double>();
            solutions = new List<Solution>();
            inputs = new List<int[]>();
            server.Start();
            StartListener();
            Console.WriteLine("Starting iterations");
            StartMain();
        }

        /// <summary>
        /// Starts the main thread that manages the clients and gives them work
        /// </summary>
        private void StartMain()
        {
            const double mutationProbability = 0.08;
            const double randomProbability = 0.001;
            for(int i =0; i < start.Count; i++)
            {
                var chromosome = new int[11];
                for(int j = 0; j < 11; j++)
                {
                    chromosome[j] = rng.Next(5000); 
                }
                //inputs.Add(chromosome);
                inputs.Add(new int[] { 100, 50000000 / 350, 1000, 100, 2000, 1000, 1000, 3000, 70, 20, 10 });
            }
            foreach (var variable in inputs)
            {
                for (int i = 0; i < variable.Length; i++)
                {
                    double mutate = rng.NextDouble();
                    if (mutate < randomProbability)
                    {
                        variable[i] = rng.Next(5000);
                    }
                    else if (mutate < mutationProbability)
                    {
                        bool positive = rng.Next(100) < 50;
                        if (positive)
                        {
                            variable[i] += rng.Next(50);
                        }
                        else
                        {
                            variable[i] -= rng.Next(50);
                        }
                    }
                }
            }

            while (true)
            {
                bool finish = true;
                foreach (bool finishId in finished)
                {
                    if (finishId == false)
                        finish = false;
                }

                if(finish)
                {
                    var index = new int[5] { 0, 0, 0, 0, 0 };
                    var temp = new List<double>();
                    var values = new double[5] { double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue };
                    for (int i = 0; i < results.Count; i++)
                    {
                        temp.Add(results[i]);
                    }
                    temp.Sort();
                    for(int i = 0; i < 5; i++)
                    {
                        values[i] = temp[i];
                        index[i] = results.FindIndex(x => x == temp[i]);
                    }
                    Console.WriteLine("Current best value = " + results[index[0]]);

                    //Generate new inputs
                    for(int i = 0; i < inputs.Count; i++)
                    {
                        var parents = new int[5] { 0, 1, 2, 3, 4 };
                        int parent1 = index[parents[rng.Next(5)]];
                        parents = parents.Where(x => x!= i).ToArray();
                        int parent2 = index[parents[rng.Next(4)]];
                        var newInput = new int[11];
                        for(int j =0; j<11; j++)
                        {
                            int rndChoice = rng.Next(100);
                            if(rndChoice <= 32)
                            {
                                newInput[j] = inputs[parent1][j];
                            }
                            else if(rndChoice <= 64)
                            {
                                newInput[j] = inputs[parent2][j];
                            }
                            else
                            {
                                newInput[j] = (inputs[parent1][j] + inputs[parent2][j]) / 2;
                            }
                        }
                        //inputs[i] = newInput;
                        inputs[i] = new int[] {100, 50000000/350, 1000, 100, 2000, 1000, 1000, 3000, 70, 20, 10 };
                    }
                    foreach (var variable in inputs)
                    {
                        for (int i = 0; i < variable.Length; i++)
                        {
                            double mutate = rng.NextDouble();
                            if (mutate < randomProbability)
                            {
                                variable[i] = rng.Next(5000);
                            }
                            else if (mutate < mutationProbability)
                            {
                                bool positive = rng.Next(100) < 50;
                                if (positive)
                                {
                                    variable[i] += rng.Next(50);
                                }
                                else
                                {
                                    variable[i] -= rng.Next(50);
                                }
                            }
                        }
                    }

                    getSolution[index[0]] = true;

                    for (int i = 0; i < finished.Count; i++)
                        finished[i] = false;
                }
                bool starting = true;
                for(int i = 0; i < start.Count; i++)
                {
                    if (start[i] == true || getSolution[i] == true || finished[i] == true)
                        starting = false;
                }
                if(starting)
                {
                    for (int i = 0; i < start.Count; i++)
                        start[i] = true;    
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Looks for incomming tcp connections. If the escape key is pressed it will stop looking
        /// </summary>
        public void StartListener()
        {
            try
            {
                var s = new Stopwatch();
                s.Start();
                Console.WriteLine("Waiting for a connection...");
                while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                {
                    if (!server.Pending())
                        continue;
                    var client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    //Thread t = new Thread(new ParameterizedThreadStart(HandleDeivce));
                    //t.Start(client);
                    finished.Add(false);
                    start.Add(false);
                    getSolution.Add(false);
                    results.Add(0);
                    lock (lockObject)
                    {
                        new Thread(() =>
                        {
                            var tClient = client;
                            var stream = tClient.GetStream();
                            int id = counter++;
                            while (true)
                            {
                                if (start[id] && !getSolution[id])
                                {
                                    //send message
                                    string message = "";
                                    for(int i =0; i < inputs[id].Length - 1; i++)
                                    {
                                        message += inputs[id][i].ToString() + ";";
                                    }
                                    message += inputs[id][inputs[id].Length -1].ToString();
                                    sendMessage(message);
                                    bool reading = true;
                                    while (reading)
                                    {
                                        if (stream.DataAvailable)
                                        {
                                            var data = new byte[256];
                                            double response;
                                            int bytes = stream.Read(data, 0, data.Length);

                                            //Amount of data it needs
                                            if (double.TryParse(Encoding.ASCII.GetString(data, 0, bytes), out response))
                                            {
                                                results[id] = response;
                                            }
                                            else
                                                continue;
                                            reading = false;
                                            finished[id] = true;
                                            start[id] = false;

                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                if (getSolution[id] && !start[id])
                                {
                                    sendMessage("Solution");
                                    Console.WriteLine("Sending request for solution");
                                    bool reading = true;
                                    best = "";
                                    while (reading)
                                    {
                                        if (stream.DataAvailable)
                                        {
                                            var data = new byte[256];

                                            int bytes = stream.Read(data, 0, data.Length);
                                            string response = Encoding.ASCII.GetString(data, 0, bytes);
                                            //Amount of data it needs
                                            if (response.Substring(response.Length - 3) == "END")
                                            {
                                                best += response.Substring(0, response.Length - 3);
                                                reading = false;
                                            }
                                            else
                                            {
                                                best += response;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    using (var outputFile = File.AppendText("WriteLines.txt"))
                                    {
                                        outputFile.WriteLine("-------------------------------------------------");
                                        outputFile.WriteLine(inputs[id].ToString());
                                        outputFile.WriteLine(results[id].ToString());
                                        outputFile.Write(best);
                                    }
                                    getSolution[id] = false;
                                }
                                Thread.Sleep(1000);
                            }
                            void sendMessage(string message)
                            {
                                var data = System.Text.Encoding.ASCII.GetBytes(message);
                                stream.Write(data, 0, data.Length);
                            }
                        }).Start();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();
            }
        }

        public void HandleDeivce(Object obj)
        {
            var client = (TcpClient)obj;
            var stream = client.GetStream();
            string data = null;
            var bytes = new byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
                    string str = "Hey Device!";
                    var reply = System.Text.Encoding.ASCII.GetBytes(str);
                    stream.Write(reply, 0, reply.Length);
                    Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                client.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace groteOpdracht
{
    class Program
    {
        static LocalSearch ls;
       public  static string fileLoc;
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("us");
            Console.WriteLine("Networked connection?");
            string networked = Console.ReadLine();

            if (networked == "n")
            {
                var ls = new LocalSearch();
                Console.WriteLine("Do you want to reed a solution? y/n");
                if (Console.ReadLine() == "n")
                    ls.currentSolution = new Solution();
                else
                {
                    Console.WriteLine("Which solution?");
                    fileLoc = Console.ReadLine() + ".txt";
                    ls.currentSolution = ls.LoadSolution(fileLoc);
                }
                ls.iterate();
                ls.PrintSolution(ls.bestSolution);
                Console.ReadLine();
                Console.SetOut(new System.IO.StreamWriter("sol.txt"));
                ls.PrintSolution(ls.bestSolution);
                Console.ReadLine();
            }
            else
            {

                Console.WriteLine("What ip adress?");
                string ip = Console.ReadLine();
                Console.WriteLine("What port?");
                int port = int.Parse(Console.ReadLine());
                Console.WriteLine("Are you the server? y/n");
                if (Console.ReadLine() == "y")
                {
                    var server = new Server(ip, port);
                    Console.CancelKeyPress += delegate
                    {
                        server.serverstart = false;
                    };
                }
                else
                {
                    var client = new Client(ip, port);
                }
            }
        }
    }
}

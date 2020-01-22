using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace groteOpdracht
{
    class LocalSearch
    {
        public static bool indexError = false;
        public static bool DEBUG = false;
        public static Dictionary<int, Order> OrderDict = new Dictionary<int, Order>();
        public static Dictionary<int, List<int>> PlaceDict = new Dictionary<int, List<int>>();
        public static Dictionary<int, List<int>> MatrixDict = new Dictionary<int, List<int>>();
        public static int[,] DistanceMatrix = new int[1099,1099];
        public Solution bestSolution, currentSolution;

        private Random rng = new Random();
        private int[] _chances;        
        private static double[,] testMatrix;


        List<string> cities = new List<string> { "BUDEL", "DEURNE", "MAARHEEZE", "LIEROP", "LIESSEL", "SOMEREN", "ASTEN", "STERKSEL", "SOERENDONK", "BUDELDORPLEIN", "MIERLO", "BEEKENDONK", "MILHEEZE", "AARLERIXTEL", "OMMEL", "SOMEREN-HEIDE", "NEERKANT", "HEUSDENGEMASTEN", "LIESHOUT", "SOMEREN-EIND", "MARIAHOUT", "BUDEL-SCHOOT", "DERIPS", "VLIERDEN", "GASTEL", "BAKEL", "HELENAVEEN", "NEDERWEERT", "GRIENDTSVEEN" };
        readonly List<List<int>> _matrices = new List<List<int>>();

        /// <summary>
        /// Runs localsearch
        /// </summary>
        public LocalSearch()
        {
            LoadOrderFile("../../orderbestand.txt");
            LoadDistanceMatrix("../../afstandenmatrix.txt");
            if (!DEBUG)
                return;

            //rng = new Random(65465494);
            rng = new Random(10);
            return;
            testMatrix = new double[cities.Count,cities.Count];
            for(int x = 0; x < cities.Count; x++)
            {
                for(int y = 0; y < cities.Count; y++)
                {
                    if (x == y)
                        continue;
                    var fromCities = _matrices[x];
                    var toCities = _matrices[y];
                    double res = 0;
                    int counter = 0;
                    foreach (int from in fromCities)
                    {
                        foreach (int to in toCities)
                        {
                            res += DistanceMatrix[@from, to];
                            counter++;
                        }
                    }
                    testMatrix[x, y] = res / counter;
                }
            }

            for(int y = 0; y < testMatrix.GetLength(1); y++)
            {
                string res = "";
                for(int x = 0; x < testMatrix.GetLength(0); x++)
                {
                    res += testMatrix[x, y].ToString() + " ";
                }
                Console.WriteLine(res);
            }
            Console.ReadLine();
        }

        public void iterate(ulong maxIterations = 50000000)
        {
            _chances = new int[] { 1000, 100, 2000, 1000, 1000, 300, 500, 200 , 150 };
            //chances = new int[] { 1091, 1663, 862, 709, 4229, 3974, 4227, 1878, 2736, 2146, 4172 };
            //chances = new int[] { 4635, 4474, 3027, 1485, 3304, 2632, 3580, 2253, 3429, 2918, 4177 };
            ulong temperatureSteps = maxIterations / 350;
            bestSolution = currentSolution.Copy();

            ulong lastBigImprovement = 0;

            var cumSum = new int[_chances.Length];
            cumSum[0] = _chances[0];
            for(int i = 1; i < _chances.Length; i++)
            {
                cumSum[i] = cumSum[i - 1] + _chances[i];
            }

            int chanceCount = _chances.Sum();
            int super = 0;
            for (int i = 0; i < 50; i++)
            {
                currentSolution = bestSolution.Copy();
                if (currentSolution.Value < 350000)
                    currentSolution.T = 75;
                else if (currentSolution.Value < 380000)
                    currentSolution.T = 125;
                else if (currentSolution.Value < 400000)
                    currentSolution.T = 150;
                else if (currentSolution.Value < 450000)
                    currentSolution.T = 250;
                else
                    currentSolution.T = 500;
                Console.WriteLine(bestSolution.Value);
                
                ulong lastImprovement = 0;
                ulong counter = 0; ;
                if (lastBigImprovement >= maxIterations * 4)
                {
                    currentSolution = new Solution();
                    lastBigImprovement = 0;
                }
                while(lastImprovement < maxIterations)
                {
                    //if (super == 2925)
                    //    ;
                    //try
                    //{
                        if (counter % temperatureSteps == 0)
                            currentSolution.T *= 0.99d;
                        int randomChoice = rng.Next(chanceCount);
                        // Add
                        if (randomChoice <= cumSum[0])
                            currentSolution.Mutate(0);
                        // Remove
                        else if (randomChoice <= cumSum[1])
                            currentSolution.Mutate(1);
                        // Shift order in trip
                        else if (randomChoice <= cumSum[2])
                            currentSolution.Mutate(2);
                        // Shift order between trips
                        else if (randomChoice <= cumSum[3])
                            currentSolution.Mutate(3);
                        // Shift order between trucks
                        else if (randomChoice <= cumSum[4])
                            currentSolution.Mutate(4);
                        // 2-Opt
                        else if (randomChoice <= cumSum[5])
                            currentSolution.Mutate(5);
                        // Add order after same place
                        else if (randomChoice <= cumSum[6])
                            currentSolution.Mutate(6);
                        // Add order after same matrixId
                        else if (randomChoice <= cumSum[7])
                            currentSolution.Mutate(7);
                        // Add order next to same matrixId, no matrixId found look for place name, no place found random add
                        else
                            currentSolution.Mutate(8);
                    //}
                    //catch
                    //{
                    //    Console.WriteLine(super);
                    //    Console.ReadLine();
                    //}
                    //foreach (var order in OrderDict)
                    //{
                    //    if (order.Value.Freq != order.Value.Locations.Count && order.Value.Locations.Count != 0)
                    //        ;
                    //}

                    //for (int x = 0; x < 2; x++)
                    //{
                    //    var truck = x == 0 ? currentSolution.Truck1 : currentSolution.Truck2;
                    //    foreach (var day in truck.Days)
                    //    {
                    //        foreach (var stop in day.UnsortedStops)
                    //        {
                    //            //if (stop.List == null)
                    //            //{
                    //            //    Console.WriteLine(super);
                    //            //    Console.ReadLine();
                    //            //}
                    //            var ord = OrderDict[stop.Value];
                    //            if (ord.Locations.Count != 0)
                    //            {
                    //                foreach(var loc in ord.Locations)
                    //                {
                    //                    if (ord.Id == 26836)
                    //                        ;
                    //                    if (ord.nodes[loc.Item2] == null)
                    //                    {
                    //                        Console.WriteLine(super);
                    //                        //Console.ReadLine();
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //if (OrderDict[26836].Locations.Count != 0 && OrderDict[26836].nodes[3] == null)
                    //    ;

                    if (currentSolution.Value < bestSolution.Value)
                    {
                        bestSolution = currentSolution.Copy();
                        Console.WriteLine(bestSolution.Value/60);
                        lastImprovement = 0;
                        lastBigImprovement = 0;
                    }
                    lastImprovement++;
                    lastBigImprovement++;
                    counter++;
                    if (indexError)
                        currentSolution = currentSolution.Copy();
                    super++;
                    if (super % 100000000 == 0)
                        Console.WriteLine(super);
                    
                }
            }
        }

        public void iterate(string[] variables, ulong maxIterations = 50000000)
        {
            _chances = new int[] { int.Parse(variables[2]), int.Parse(variables[3]), int.Parse(variables[4]), int.Parse(variables[5]), int.Parse(variables[6]), int.Parse(variables[7]), int.Parse(variables[8]), int.Parse(variables[9]), int.Parse(variables[10]) };
            ulong temperatureSteps = ulong.Parse(variables[1]);
            currentSolution = new Solution();//startSolution();
            bestSolution = currentSolution.Copy();


            ulong lastImprovement = 0;
            ulong lastBigImprovement = 0;

            var cumSum = new int[_chances.Length];
            cumSum[0] = _chances[0];
            for (int i = 1; i < _chances.Length; i++)
            {
                cumSum[i] = cumSum[i - 1] + _chances[i];
            }



            int chanceCount = _chances.Sum();
            for (int i = 0; i < 10; i++)
            {
                currentSolution = bestSolution.Copy();
                Console.WriteLine(bestSolution.Value);
                currentSolution.T = double.Parse(variables[0]);
                lastImprovement = 0;
                ulong counter = 0;
                if (lastBigImprovement >= maxIterations * 2)
                {
                    currentSolution = new Solution();
                    lastBigImprovement = 0;
                }
                while(lastImprovement < maxIterations)
                {
                    if (counter % temperatureSteps == 0)
                        currentSolution.T *= 0.99d;
                    int randomChoice = rng.Next(chanceCount);
                    // Add
                    if (randomChoice <= cumSum[0])
                        currentSolution.Mutate(0);
                    // Remove
                    else if (randomChoice <= cumSum[1])
                        currentSolution.Mutate(1);
                    // Shift order in trip
                    else if (randomChoice <= cumSum[2])
                        currentSolution.Mutate(2);
                    // Shift order between trips
                    else if (randomChoice <= cumSum[3])
                        currentSolution.Mutate(3);
                    // Shift order between trucks
                    else if (randomChoice <= cumSum[4])
                        currentSolution.Mutate(4);
                    // 2-Opt
                    else if (randomChoice <= cumSum[5])
                        currentSolution.Mutate(5);
                    // Add order after same place
                    else if (randomChoice <= cumSum[6])
                        currentSolution.Mutate(6);
                    // Add order after same matrixId
                    else if (randomChoice <= cumSum[7])
                        currentSolution.Mutate(7);
                    // Add order next to same matrixId, no matrixId found look for place name, no place found random add
                    else
                        currentSolution.Mutate(8);
                    if (currentSolution.Value < bestSolution.Value)
                    {
                        bestSolution = currentSolution.Copy();
                        Console.WriteLine(bestSolution.Value);
                        lastImprovement = 0;
                        lastBigImprovement = 0;
                        if (currentSolution.Value <= 350000)
                            PrintSolution(bestSolution, true);
                    }
                    lastImprovement++;
                    lastBigImprovement++;
                    counter++;
                    if (indexError)
                        currentSolution = currentSolution.Copy();
                }
            }
        }


        /// <summary>
        /// Debug mathod in case of errors in penalty
        /// </summary>
        public void checkPenalty()
        {
            double res = 0;
            foreach(var order in OrderDict.Values)
            {
                if (order.Locations.Count == 0)
                    res += order.LedigingsDuur * order.Freq * 3;
            }
            if (Math.Abs(currentSolution.Penalty - res) > 0.0001f)
            {
                PrintSolution(currentSolution);
            }
        }

        /// <summary>
        /// Generate a start solution with user input
        /// </summary>
        /// <returns>A start solution</returns>
        private Solution startSolution()
        {
            var res = new Solution();
            Console.WriteLine("Start with a random Solution? y/n");
            if (Console.ReadLine() != "y")
                return res;

            Console.WriteLine("Which method? 0: Pure Random; 1: Based on location?; Default is 0");
            var choice = ToNullableInt(Console.ReadLine());
            Console.WriteLine("How many times do you want to add? Default 1000");
            var amount = ToNullableInt(Console.ReadLine());  //int.Parse(Console.ReadLine());
            amount = amount == null ? 1000 : amount;
            if (choice == 1)
            {
                for (int i = 0; i < amount; i++)
                    res.Mutate(8);
            }
            else
            {
                for (int i = 0; i < amount; i++)
                    res.Mutate(0);
            }
            return res;
        }

        // Adapted from https://stackoverflow.com/questions/45030/how-to-parse-a-string-into-a-nullable-int
        /// <summary>
        /// Converts a string to a nullable int. Is null when the input string is not an int
        /// </summary>
        /// <param name="v">The string that needs to be converted</param>
        /// <returns>A nullable int</returns>
        private int? ToNullableInt(string v)
        {
            int i;
            if (int.TryParse(v, out i)) return i;
            return null;
        }

        /// <summary>
        /// Loads orders from a file
        /// </summary>
        /// <param name="path">Path were the file is located</param>
        public void LoadOrderFile(string path)
        {
            for (int i = 0; i < cities.Count; i++)
                _matrices.Add(new List<int>());
            
            string[] orderbestand = Properties.Resources.orderbestand.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < orderbestand.Length; i++)
            {
                string rij = orderbestand[i];
                var order = new Order();
                string[] data = rij.Split(';');
                order.Id = int.Parse(data[0]);
                int plaats = cities.FindIndex(x => x == new string(data[1].Where(y => y != ' ').ToArray()));
                order.Plaats = plaats;
                order.Freq = int.Parse(data[2][0].ToString());
                order.AantalContainers = int.Parse(data[3]);
                order.Volume = int.Parse(data[4]);
                order.LedigingsDuur = double.Parse(data[5]) * 60;
                order.MatrixId = int.Parse(data[6]);
                OrderDict.Add(order.Id, order);
                if (!MatrixDict.ContainsKey(order.MatrixId))
                    MatrixDict.Add(order.MatrixId, new List<int>());
                MatrixDict[order.MatrixId].Add(order.Id);

                if (!PlaceDict.ContainsKey(plaats))
                    PlaceDict.Add(plaats, new List<int>());
                PlaceDict[plaats].Add(order.Id);
                  
            }
        }


        /// <summary>
        /// Loads distance matrix from a file
        /// </summary>
        /// <param name="path">Path were the file is located</param>
        public void LoadDistanceMatrix(string path)
        {
            string[] afstanden = Properties.Resources.afstandenmatrix.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < afstanden.Length; i++)
            {
                var data = afstanden[i].Split(';');
                int a = int.Parse(data[0]);
                int b = int.Parse(data[1]);
                int c = int.Parse(data[3]);

                DistanceMatrix[a, b] = c;
            }
        }
        
        /// <summary>
        /// Loads a solution from a given path
        /// </summary>
        /// <param name="path">Path were the file is located</param>
        /// <returns>Returns the loaded solution</returns>
        // TODO: FIX
        public Solution LoadSolution(string path)
        {
            var reader = new StreamReader(path);
            var res = new Solution();
            // Truck, dag, hoeveel, orderid
            int prev = 1;
            int trip = 0;
            while (reader.Peek() != -1)
            {
                var data = reader.ReadLine().Split(';');
                int truck = int.Parse(data[0]); 

                if (prev != truck)
                {
                    trip = 0;
                    prev = truck;
                }
                if (int.Parse(data[3]) == 0)
                {
                    trip = 1;
                    continue;
                }
                res.AddOrders(int.Parse(data[3]), new List<(int, int, int)> { (int.Parse(data[0]) -1, int.Parse(data[1]) - 1, trip)}, true);
            }
            reader.Close();
            return res;
        }

        /// <summary>
        /// Prints the solution to the console
        /// </summary>
        /// <param name="solution">The solution that needs to be printed</param>
        public void PrintSolution(Solution solution, bool save = false)
        {
            if(save)
            {
                using (var outputFile = File.AppendText("WriteAnswer.txt"))
                {
                    outputFile.WriteLine("-------------------------------------------------");
                    for (int i = 0; i < 5; i++)
                    {
                        int counter = 1;
                        for (int j = 0; j < 2; j++)
                        {
                            var trip = solution.Truck1.Days[i, j];
                            var node = trip.Stops.First;
                            while( node.Next != null)
                            {
                                
                            }
                            for (int x = 0; x < trip.Stops.Count; x++)
                            {
                                outputFile.WriteLine(1 + ";" + (i + 1) + ";" + (counter++) + ";" + node.Value);
                                node = node.Next;
                            }
                            outputFile.WriteLine(1 + ";" + (i + 1) + ";" + (counter++) + ";" + 0);
                        }
                        counter = 1;
                        for (int j = 0; j < 2; j++)
                        {
                            var trip = solution.Truck2.Days[i, j];
                            var node = trip.Stops.First;
                            for (int x = 0; x < trip.Stops.Count; x++)
                            {
                                outputFile.WriteLine(2 + ";" + (i + 1) + ";" + (counter++) + ";" + node.Value);
                                node = node.Next;
                            }
                            outputFile.WriteLine(2 + ";" + (i + 1) + ";" + (counter++) + ";" + 0);

                        }
                    }
                    outputFile.WriteLine("-------------------------------------------------");
                }
            }
            else
            {
                Console.Clear();
                for(int i = 0; i < 5; i++)
                {
                    int counter = 1;
                    for (int j = 0; j < 2; j++)
                    {
                        var trip = solution.Truck1.Days[i,j];
                        var node = trip.Stops.First;
                        for(int x = 0; x < trip.Stops.Count; x++)
                        {
                            Console.WriteLine(1 + ";" + (i + 1) + ";" + (counter++) + ";" + node.Value);
                            node = node.Next;
                        }
                        Console.WriteLine(1 + ";" + (i + 1) + ";" + (counter++) + ";" + 0);
                    }
                    counter = 1;
                    for (int j = 0; j < 2; j++)
                    {
                        var trip = solution.Truck2.Days[i,j];
                        var node = trip.Stops.First;
                        for (int x = 0; x < trip.Stops.Count; x++)
                        {
                            Console.WriteLine(2 + ";" + (i + 1) + ";" + (counter++) + ";" + node.Value);
                            node = node.Next;
                        }
                        Console.WriteLine(2 + ";" + (i + 1) + ";" + (counter++) + ";" + 0);

                    }
                }

            }
        }
    }
}

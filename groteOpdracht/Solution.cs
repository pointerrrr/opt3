using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static groteOpdracht.LocalSearch;

namespace groteOpdracht
{
    class Solution
    {
        public double Penalty, T = 100;

        /// <summary>
        /// Returns total value of a solution
        /// </summary>
        public double Value
        {
            get
            {
                return Penalty + DriveTime;
            }
        }
        /// <summary>
        /// Returns the total drivetime of a solution
        /// </summary>
        public double DriveTime
        {
            get
            {
                return Truck1.DriveTime + Truck2.DriveTime;
            }
        }

        public Truck Truck1, Truck2;
        Random rng = new Random();

        /// <summary>
        /// Checks if a solution is valid
        /// </summary>
        public bool Valid
        {
            get
            {
                return Truck1.Valid && Truck2.Valid;
            }
        }

        /// <summary>
        /// Generates a solution and calculates the penalty
        /// </summary>
        /// <param name="temp"></param>
        public Solution(double temp = 100)
        {
            Truck1 = new Truck();
            Truck2 = new Truck();
            T = temp;
            foreach(var kvp in OrderDict)
            {
                Penalty += kvp.Value.LedigingsDuur * kvp.Value.Freq * 3;
            }
            if (DEBUG)
                rng = new Random(10);
        }

        /// <summary>
        /// Mutates the solution with a given neighbour space. 
        /// 0: add; 1: remove; 2: shift in trip; 
        /// 3: shift between trips; 4: shift between trucks; 5: 2-opt;
        /// 6: add same place; 7: add same matrixid; 8: combine 6 and 7
        /// </summary>
        /// <param name="mutation">Kind of mutation that needs to be executed</param>
        public void Mutate(int mutation)
        {
            switch (mutation)
            {
                case 0:
                    //Add order
                    var order = OrderDict.ElementAt(rng.Next(OrderDict.Count)).Value;
                    if (order.Locations.Count > 0)
                        break;
                    var locations = new List<(int, int, int)>();
                    var dagen = new int[order.Freq];

                    switch (order.Freq)
                    {
                        case 1:
                            dagen = new int[] { rng.Next(5) };
                            break;
                        case 2:
                            dagen = rng.Next(2) == 0 ? new int[] { 0, 3 } : new int[] { 1, 4 };
                            break;
                        case 3:
                            dagen = new int[] { 0, 2, 4 };
                            break;
                        case 4:
                            dagen = new int[] { 0, 1, 2, 3, 4 };
                            int index = rng.Next(5);
                            dagen = dagen.Where(x => x != index).ToArray();
                            break;
                    }

                    for (int i = 0; i < order.Freq; i++)
                    {
                        int truck = rng.Next(2);
                        int dag = dagen[i];
                        int trip = rng.Next(2);
                        locations.Add((truck, dag, trip));
                    }
                    (double newValue, bool valid) = CheckAddOrders(order.Id, locations);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        AddOrders(order.Id, locations);
                        if (!Valid)
                            RemoveOrders(order.Id);
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 1:
                    //Remove order
                    order = OrderDict.ElementAt(rng.Next(OrderDict.Count)).Value;
                    if (order.Locations.Count == 0)
                        break; 
                    var tempLoc = new List<(int, int, int)>();
                    foreach (var loc in order.Locations)
                        tempLoc.Add((loc.Item1, loc.Item2, loc.Item3));
                    (newValue, valid) = CheckRemoveOrder(order.Id);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        RemoveOrders(order.Id);
                        if (!Valid)
                        {
                            AddOrders(order.Id, tempLoc);
                        }
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 2:
                    //Shift order in trip
                    int trucka = rng.Next(2);
                    int daga = rng.Next(5);
                    int tripa = rng.Next(2);
                    var location = (trucka, daga, tripa);
                    var curTruck = trucka == 0 ? Truck1 : Truck2;
                    int max = curTruck.Days[daga, tripa].Stops.Count;
                    if (max <= 1)
                        break;
                    int fromIndex = rng.Next(max);
                    int toIndex = -1;
                    //if (rng.Next(4) == 0)
                    //{
                    //    var tempOrder = OrderDict[curTruck.Days[daga, tripa].UnsortedStops[fromIndex].Value];

                    //    for(int i = 0; i < max; i++)
                    //    {
                    //        if (tempOrder.MatrixId == OrderDict[curTruck.Days[daga, tripa].UnsortedStops[i].Value].MatrixId && i != fromIndex)
                    //            toIndex = i;
                    //    }
                    //    if(toIndex == -1)
                    //    {
                    //        for (int i = 0; i < max; i++)
                    //        {
                    //            if (tempOrder.Plaats == OrderDict[curTruck.Days[daga, tripa].UnsortedStops[i].Value].Plaats && i != fromIndex)
                    //                toIndex = i;
                    //        }
                    //    }
                    //    if (toIndex == -1)
                    //        toIndex = rng.Next(max);
                    //}
                    //else
                    //{
                        toIndex = rng.Next(max);
                    //}
                    /*toIndex -1 == fromIndex*/
                    if (curTruck.Days[daga, tripa].UnsortedStops[fromIndex].Next == curTruck.Days[daga, tripa].UnsortedStops[toIndex])
                    {
                        int temp = toIndex;
                        toIndex = fromIndex;
                        fromIndex = temp;
                    }
                    (newValue, valid) = CheckShiftOrder(location, fromIndex, toIndex);

                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        ShiftOrder(location, fromIndex, toIndex);
                        if (!Valid)
                        {
                            if (fromIndex < toIndex)
                                ShiftOrder(location, toIndex - 1, fromIndex);
                            else if (fromIndex - 1 == toIndex)
                                ShiftOrder(location, toIndex, fromIndex + 1);
                            else
                                ShiftOrder(location, toIndex, fromIndex + 1);
                        }
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 3:
                    //Shift order between trips
                    // Delete from location 1, add at location 2
                    int truckb = rng.Next(2);
                    int dagb = rng.Next(5);
                    int tripb1 = rng.Next(2);
                    int tripb2 = tripb1 == 0 ? 1 : 0;
                    locations = new List<(int, int, int)>();
                    locations.Add((truckb, dagb, tripb1));
                    locations.Add((truckb, dagb, tripb2));
                    var curTruckb = truckb == 0 ? Truck1 : Truck2;
                    int maxb1 = curTruckb.Days[dagb, tripb1].Stops.Count;
                    int maxb2 = curTruckb.Days[dagb, tripb2].Stops.Count;
                    if (maxb1 == 0)
                        break ;
                    int removeIndex = rng.Next(maxb1);
                    int addIndex = rng.Next(maxb2);
                    (newValue, valid) = CheckShiftTrip(locations, removeIndex, addIndex);

                    if((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        ShiftTrip(locations, removeIndex, addIndex);
                        if (!Valid)
                        {
                            var tempLocations = new List<(int, int, int)>();
                            tempLocations.Add(locations[1]);
                            tempLocations.Add(locations[0]);
                            int temp = removeIndex;
                            removeIndex = addIndex;
                            addIndex = temp;
                            ShiftTrip(tempLocations, removeIndex, addIndex);
                        }
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 4:
                    //Shift order between trucks
                    int truckc1 = rng.Next(2);
                    int truckc2 = truckc1 == 0 ? 1 : 0;
                    int dagc = rng.Next(5);
                    int tripc1 = rng.Next(2);
                    int tripc2 = rng.Next(2);

                    locations = new List<(int, int, int)>();
                    locations.Add((truckc1, dagc, tripc1));
                    locations.Add((truckc2, dagc, tripc2));

                    var curTruckc1 = truckc1 == 0 ? Truck1 : Truck2;
                    var curTruckc2 = truckc2 == 0 ? Truck1 : Truck2;

                    int maxc1 = curTruckc1.Days[dagc, tripc1].Stops.Count;
                    int maxc2 = curTruckc2.Days[dagc, tripc2].Stops.Count;

                    if (maxc1 == 0)
                        break;
                    int removeIndexC = rng.Next(maxc1);
                    int addIndexC = rng.Next(maxc2);
                    (newValue, valid) = CheckShiftTrip(locations, removeIndexC, addIndexC);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        ShiftTrip(locations, removeIndexC, addIndexC);
                        if (!Valid)
                        {
                            var tempLocations = new List<(int, int, int)>();
                            tempLocations.Add(locations[1]);
                            tempLocations.Add(locations[0]);
                            int temp = removeIndexC;
                            removeIndexC = addIndexC;
                            addIndexC = temp;
                            ShiftTrip(tempLocations, removeIndexC, addIndexC);
                        }
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 5:
                    //2-opt
                    int truckD = rng.Next(2);
                    int dagD = rng.Next(5);
                    int tripD = rng.Next(2);

                    var curTruckD = truckD == 0 ? Truck1 : Truck2;

                    int maxD = curTruckD.Days[dagD, tripD].Stops.Count;
                    //You need tow nodes to determine where and then two extra nodes to swap. otherwise it is either not possible or not neccesary
                    if (maxD < 4)
                        break;
                    var locationD =(truckD,dagD,tripD);
                    int swapIndex1 = rng.Next(maxD);
                    int swapIndex2 = rng.Next(maxD);

                    if (Math.Abs(swapIndex1 - swapIndex2) < 4)
                        break;
                    if(swapIndex1 > swapIndex2)
                    {
                        int temp = swapIndex1;
                        swapIndex1 = swapIndex2;
                        swapIndex2 = temp;
                    }
                    var tempValue = Check2Opt(locationD, swapIndex1, swapIndex2);
                    double oldValue = curTruckD.Days[dagD, tripD].Duration;
                    newValue = Value + (tempValue.Item1 - curTruckD.Days[dagD, tripD].Duration);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && tempValue.Item2)
                    {
                        twoOpt(locationD, swapIndex1, swapIndex2, tempValue.Item1);
                        if (!Valid)
                        {
                            twoOpt(locationD, swapIndex1, swapIndex2, oldValue);
                        }
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 6:
                    //Add order after same place
                    var orderE = OrderDict.ElementAt(rng.Next(OrderDict.Count)).Value;
                    if (orderE.Locations.Count > 0)
                        break;
                    var locationsE = new List<(int, int, int, LinkedListNode<int>)>();
                    var dagenE = new int[orderE.Freq];

                    switch (orderE.Freq)
                    {
                        case 1:
                            dagenE = new int[] { rng.Next(5) };
                            break;
                        case 2:
                            dagenE = rng.Next(2) == 0 ? new int[] { 0, 3 } : new int[] { 1, 4 };
                            break;
                        case 3:
                            dagenE = new int[] { 0, 2, 4 };
                            break;
                        case 4:
                            dagenE = new int[] { 0, 1, 2, 3, 4 };
                            int index = rng.Next(5);
                            dagenE = dagenE.Where(x => x != index).ToArray();
                            break;
                    }

                    for (int i = 0; i < orderE.Freq; i++)
                    {


                        int dag = dagenE[i];

                        var cities = PlaceDict[orderE.Plaats];
                        var randOrder = OrderDict[cities[rng.Next(cities.Count)]];
                        if (randOrder.Locations.Count == 0 || randOrder.Id == orderE.Id)
                            return;

                        bool same = false;
                        int ind = -1;
                        int truck = rng.Next(2);
                        int trip = rng.Next(2);
                        for(int j = 0; j < randOrder.Freq; j++)
                        {
                            if (randOrder.Locations[j].Item2 == dag)
                            {
                                same = true;
                                ind = j;
                                truck = randOrder.Locations[j].Item1;
                                trip = randOrder.Locations[j].Item3;
                                break;
                            }
                            
                        }
                        if (same)
                        {
                            locationsE.Add((truck, dag, trip, randOrder.nodes[dag]));
                        }
                        else
                        {
                            var rTruck = truck == 0 ? Truck1 : Truck2;
                            if (rTruck.Days[dag, trip].UnsortedStops.Count == 0)
                                return;
                            var randNode = rTruck.Days[dag, trip].UnsortedStops[rng.Next(rTruck.Days[dag, trip].UnsortedStops.Count)];
                            locationsE.Add((truck, dag, trip, randNode));

                        }
                        //int truck = -1;
                        //int trip = -1;
                        //int pos = -1;
                        ////check each truck
                        //for(int j = 0; j < 2; j++)
                        //{
                        //    //check each trip
                        //    for (int x = 0; x < 2; x++)
                        //    {
                        //        // get current truck and current trip
                        //        var curTruckE = j == 0 ? Truck1 : Truck2;
                        //        var curTrip = curTruckE.Days[dag, x];
                        //        for (int y = 0; y < curTrip.Stops.Count; y++)
                        //        {
                        //            if(OrderDict[curTrip.UnsortedStops[y].Value].Plaats == orderE.Plaats)
                        //            {
                        //                truck = j;
                        //                trip = x;
                        //                pos = y;
                        //                break;
                        //            }
                        //        }
                        //        if (truck != -1)
                        //            break;
                        //    }
                        //    if (truck != -1)
                        //        break;
                        //}
                        //if(truck == -1)
                        //{
                        //    truck = rng.Next(2);
                        //    trip = rng.Next(2);
                        //    pos = truck == 0 ? rng.Next(Truck1.Days[dag, trip].Stops.Count) : rng.Next(Truck2.Days[dag, trip].Stops.Count);
                        //}
                        //locationsE.Add((truck, dag, trip, pos));
                    }
                    (newValue, valid) = CheckAddOrders(orderE.Id, locationsE);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        AddOrders(orderE.Id, locationsE);
                        if (!Valid)
                            RemoveOrders(orderE.Id);
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }

                    break;
                case 7:
                    //Add order after same matrixId
                    var orderF = OrderDict.ElementAt(rng.Next(OrderDict.Count)).Value;
                    if (orderF.Locations.Count > 0)
                        break;
                    var locationsF = new List<(int, int, int, LinkedListNode<int>)>();
                    var dagenF = new int[orderF.Freq];

                    switch (orderF.Freq)
                    {
                        case 1:
                            dagenF = new int[] { rng.Next(5) };
                            break;
                        case 2:
                            dagenF = rng.Next(2) == 0 ? new int[] { 0, 3 } : new int[] { 1, 4 };
                            break;
                        case 3:
                            dagenF = new int[] { 0, 2, 4 };
                            break;
                        case 4:
                            dagenF = new int[] { 0, 1, 2, 3, 4 };
                            int index = rng.Next(5);
                            dagenF = dagenF.Where(x => x != index).ToArray();
                            break;
                    }

                    for (int i = 0; i < orderF.Freq; i++)
                    {
                        int dag = dagenF[i];

                        var matrices = MatrixDict[orderF.MatrixId];
                        bool same = false;
                        int ind = -1;
                        int truck = rng.Next(2);
                        int trip = rng.Next(2);
                        var curOrder = OrderDict[matrices[0]];
                        for (int j = 0; j < matrices.Count; j++)
                        {
                            curOrder = OrderDict[matrices[j]];
                            if (curOrder.Id == orderF.Id || curOrder.Locations.Count == 0)
                                continue;

                            for(int x = 0; x < curOrder.Freq; x++)
                            {
                                if(curOrder.Locations[x].Item2 == dag)
                                {
                                    same = true;
                                    ind = x;
                                    truck = curOrder.Locations[x].Item1;
                                    trip = curOrder.Locations[x].Item3;
                                    break;
                                }
                            }

                            if (same)
                                break;
                        }

                        if (same)
                            locationsF.Add((truck, dag, trip, curOrder.nodes[dag]));
                        else
                        {
                            var rTruck = truck == 0 ? Truck1 : Truck2;
                            if (rTruck.Days[dag, trip].UnsortedStops.Count == 0)
                                return;
                            var randNode = rTruck.Days[dag, trip].UnsortedStops[rng.Next(rTruck.Days[dag, trip].UnsortedStops.Count)];
                            locationsF.Add((truck, dag, trip, randNode));
                        }

                        //var randOrder = OrderDict[matrices[rng.Next(matrices.Count)]];
                        //if (randOrder.Locations.Count == 0)
                        //    return;

                        //bool same = false;
                        //int ind = -1;
                        //int truck = rng.Next(2);
                        //int trip = rng.Next(2);
                        //for (int j = 0; j < randOrder.Freq; j++)
                        //{
                        //    if (randOrder.Locations[j].Item2 == dag)
                        //    {
                        //        same = true;
                        //        ind = j;
                        //        truck = randOrder.Locations[j].Item1;
                        //        trip = randOrder.Locations[j].Item3;
                        //        break;
                        //    }

                        //}
                        //if (same)
                        //{
                        //    locationsE.Add((truck, dag, trip, randOrder.nodes[dag]));
                        //}
                        //else
                        //{
                        //    var rTruck = truck == 0 ? Truck1 : Truck2;
                        //    if (rTruck.Days[dag, trip].UnsortedStops.Count == 0)
                        //        return;
                        //    var randNode = rTruck.Days[dag, trip].UnsortedStops[rng.Next(rTruck.Days[dag, trip].UnsortedStops.Count)];
                        //    locationsE.Add((truck, dag, trip, randNode));

                        //}

                        //int dag = dagenF[i];
                        //int truck = -1;
                        //int trip = -1;
                        //int pos = -1;
                        ////check each truck
                        //for (int j = 0; j < 2; j++)
                        //{
                        //    //check each trip
                        //    for (int x = 0; x < 2; x++)
                        //    {
                        //        // get current truck and current trip
                        //        var curTruckF = j == 0 ? Truck1 : Truck2;
                        //        var curTrip = curTruckF.Days[dag, x];
                        //        for (int y = 0; y < curTrip.Stops.Count; y++)
                        //        {
                        //            if (OrderDict[curTrip.UnsortedStops[y].Value].MatrixId == orderF.MatrixId)
                        //            {
                        //                truck = j;
                        //                trip = x;
                        //                pos = y;
                        //                break;
                        //            }
                        //        }
                        //        if (truck != -1)
                        //            break;
                        //    }
                        //    if (truck != -1)
                        //        break;
                        //}
                        //if (truck == -1)
                        //{
                        //    //Kiezen of niks doen. Of willekeurig toevoegen
                        //    //return;

                        //    truck = rng.Next(2);
                        //    trip = rng.Next(2);
                        //    pos = truck == 0 ? rng.Next(Truck1.Days[dag, trip].Stops.Count) : rng.Next(Truck2.Days[dag, trip].Stops.Count);
                        //}
                        //locationsF.Add((truck, dag, trip, pos));
                    }
                    (newValue, valid) = CheckAddOrders(orderF.Id, locationsF);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        AddOrders(orderF.Id, locationsF);
                        if (!Valid)
                            RemoveOrders(orderF.Id);
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                case 8:
                    //Add order next to same matrixId, no matrixId found look for place name, no place found random add
                    var orderG = OrderDict.ElementAt(rng.Next(OrderDict.Count)).Value;
                    if (orderG.Locations.Count > 0)
                        break;
                    var locationsG = new List<(int, int, int, LinkedListNode<int>)>();
                    var dagenG = new int[orderG.Freq];

                    switch (orderG.Freq)
                    {
                        case 1:
                            dagenG = new int[] { rng.Next(5) };
                            break;
                        case 2:
                            dagenG = rng.Next(2) == 0 ? new int[] { 0, 3 } : new int[] { 1, 4 };
                            break;
                        case 3:
                            dagenG = new int[] { 0, 2, 4 };
                            break;
                        case 4:
                            dagenG = new int[] { 0, 1, 2, 3, 4 };
                            int index = rng.Next(5);
                            dagenG = dagenG.Where(x => x != index).ToArray();
                            break;
                    }

                    //Check for matrixId
                    for (int i = 0; i < orderG.Freq; i++)
                    {
                        int dag = dagenG[i];

                        var matrices = MatrixDict[orderG.MatrixId];
                        bool same = false;
                        int ind = -1;
                        int truck = rng.Next(2);
                        int trip = rng.Next(2);
                        var curOrder = OrderDict[matrices[0]];
                        for (int j = 0; j < matrices.Count; j++)
                        {
                            curOrder = OrderDict[matrices[j]];
                            if (curOrder.Id == orderG.Id || curOrder.Locations.Count == 0)
                                continue;

                            for (int x = 0; x < curOrder.Freq; x++)
                            {
                                if (curOrder.Locations[x].Item2 == dag)
                                {
                                    same = true;
                                    ind = x;
                                    truck = curOrder.Locations[x].Item1;
                                    trip = curOrder.Locations[x].Item3;
                                    break;
                                }
                            }

                            if (same)
                                break;
                        }

                        if(!same)
                        {
                           

                            var cities = PlaceDict[orderG.Plaats];
                            curOrder = OrderDict[cities[rng.Next(cities.Count)]];
                            if (curOrder.Locations.Count == 0 || curOrder.Id == orderG.Id)
                                return;

                           
                            ind = -1;
                            truck = rng.Next(2);
                            trip = rng.Next(2);
                            for (int j = 0; j < curOrder.Freq; j++)
                            {
                                if (curOrder.Locations[j].Item2 == dag)
                                {
                                    same = true;
                                    ind = j;
                                    truck = curOrder.Locations[j].Item1;
                                    trip = curOrder.Locations[j].Item3;
                                    break;
                                }

                            }
                        }
                        if (same)
                            locationsG.Add((truck, dag, trip, curOrder.nodes[dag]));
                        else
                        {
                            var rTruck = truck == 0 ? Truck1 : Truck2;
                            if (rTruck.Days[dag, trip].UnsortedStops.Count == 0)
                                return;
                            var randNode = rTruck.Days[dag, trip].UnsortedStops[rng.Next(rTruck.Days[dag, trip].UnsortedStops.Count)];
                            locationsG.Add((truck, dag, trip, randNode));
                        }


                        //int dag = dagenG[i];
                        //int truck = -1;
                        //int trip = -1;
                        //int pos = -1;
                        ////check each truck
                        //for (int j = 0; j < 2; j++)
                        //{
                        //    //check each trip
                        //    for (int x = 0; x < 2; x++)
                        //    {
                        //        // get current truck and current trip
                        //        var curTruckG = j == 0 ? Truck1 : Truck2;
                        //        var curTrip = curTruckG.Days[dag, x];
                        //        for (int y = 0; y < curTrip.Stops.Count; y++)
                        //        {
                        //            if (OrderDict[curTrip.UnsortedStops[y].Value].MatrixId == orderG.MatrixId)
                        //            {
                        //                truck = j;
                        //                trip = x;
                        //                pos = y;
                        //                break;
                        //            }
                        //        }
                        //        if (truck != -1)
                        //            break;
                        //    }
                        //    if (truck != -1)
                        //        break;
                        //}
                        ////No matrixId found look for place
                        //if(truck == -1)
                        //{

                        //    for (int j = 0; j < 2; j++)
                        //    {
                        //        //check each trip
                        //        for (int x = 0; x < 2; x++)
                        //        {
                        //            // get current truck and current trip
                        //            var curTruckG = j == 0 ? Truck1 : Truck2;
                        //            var curTrip = curTruckG.Days[dag, x];
                        //            for (int y = 0; y < curTrip.Stops.Count; y++)
                        //            {
                        //                if (OrderDict[curTrip.UnsortedStops[y].Value].Plaats == orderG.Plaats)
                        //                {
                        //                    truck = j;
                        //                    trip = x;
                        //                    pos = y;
                        //                    break;
                        //                }
                        //            }
                        //            if (truck != -1)
                        //                break;
                        //        }
                        //        if (truck != -1)
                        //            break;
                        //    }

                        //}

                        ////No place found random add
                        //if (truck == -1)
                        //{
                        //    //Kiezen of niks doen. Of willekeurig toevoegen
                        //    //return;

                        //    truck = rng.Next(2);
                        //    trip = rng.Next(2);
                        //    pos = truck == 0 ? rng.Next(Truck1.Days[dag, trip].Stops.Count) : rng.Next(Truck2.Days[dag, trip].Stops.Count);
                        //}
                        //locationsG.Add((truck, dag, trip, pos));
                    }
                    (newValue, valid) = CheckAddOrders(orderG.Id, locationsG);
                    if ((newValue < Value || rng.NextDouble() < AcceptationChance(Value, newValue, T)) && valid)
                    {
                        AddOrders(orderG.Id, locationsG);
                        if (!Valid)
                            RemoveOrders(orderG.Id);
                        if (Math.Abs(newValue - Value) > 0.0001f || !Valid)
                            ;
                    }
                    break;
                default:
                    return;
            }
            if (!Valid)
                ;
        }

        /// <summary>
        /// Executes a two opt mutation
        /// </summary>
        /// <param name="locationD">Truck, day, trip</param>
        /// <param name="swapIndex1">Exclusive lower index</param>
        /// <param name="swapIndex2">Exclusive upper index</param>
        /// <param name="newValue">Privious calculated new value the two opt has</param>
        private void twoOpt((int , int , int ) locationD, int swapIndex1, int swapIndex2, double newValue)
        {
            // TODO: FIX DIE SHIT
            var truck = locationD.Item1 == 0 ? Truck1 : Truck2;
            /*var newStop = new List<int>();
            var trip = truck.Days[locationD.Item2, locationD.Item3];
            for (int i = 0; i <= swapIndex1; i++)
                newStop.Add(trip.Stops[i]);
            for (int i = swapIndex2 - 1; i > swapIndex1; i--)
                newStop.Add(trip.Stops[i]);
            for (int i = swapIndex2; i < trip.Stops.Count; i++)
                newStop.Add(trip.Stops[i]);

            truck.Days[locationD.Item2, locationD.Item3].Stops =  newStop;*/

           


            var trip = truck.Days[locationD.Item2, locationD.Item3];
            var newStop = new LinkedList<int>();
            var newUnsorted = new List<LinkedListNode<int>>();
            var node1 = truck.Days[locationD.Item2, locationD.Item3].Stops.First;
            var node2 = new LinkedListNode<int>(-1);
            var nodetemp = new LinkedListNode<int>(-1);

            for (int i = 0; i < swapIndex2; i++)
            {
                if (i < swapIndex1)
                {
                    newStop.AddLast(node1);
                    newUnsorted.Add(newStop.Last);
                    OrderDict[newStop.Last.Value].nodes[locationD.Item2] = newStop.Last;
                }
                node1 = node1.Next;

            }
            node2 = node1;
            for(int i = swapIndex2; i > swapIndex1; i--)
            {
                newStop.AddLast(node1);
                node1 = node1.Previous;
                newUnsorted.Add(newStop.Last);
                OrderDict[newStop.Last.Value].nodes[locationD.Item2] = newStop.Last;
            }
            while(node2.Next != null)
            {
                newStop.AddLast(node2);
                node2 = node2.Next;
                newUnsorted.Add(newStop.Last);
                OrderDict[newStop.Last.Value].nodes[locationD.Item2] = newStop.Last;
            }
            newStop.AddLast(node2);
            newUnsorted.Add(newStop.Last);
            OrderDict[newStop.Last.Value].nodes[locationD.Item2] = newStop.Last;


            //for (int i = swapIndex2 - 1; i > swapIndex1; i--)
            //    newStop.Add(trip.Stops[i]);
            //for (int i = swapIndex2; i < trip.Stops.Count; i++)
            //    newStop.Add(trip.Stops[i]);

            //var node1 = truck.Days[locationD.Item2, locationD.Item3].UnsortedStops[swapIndex1].Value;
            //var node2 = truck.Days[locationD.Item2, locationD.Item3].UnsortedStops[swapIndex2].Value;


            truck.Days[locationD.Item2, locationD.Item3].Duration = newValue;
            truck.Days[locationD.Item2, locationD.Item3].Stops = newStop;
            truck.Days[locationD.Item2, locationD.Item3].UnsortedStops = newUnsorted;

            // INCASE of fuckup check if 2 opt still works by using this code
            //var truck = locationD.Item1 == 0 ? Truck1 : Truck2;
            //var trip = truck.Days[locationD.Item2, locationD.Item3];
            //var newTrip = new Trip();
            //for (int i = 0; i <= swapIndex1; i++)
            //    newTrip.AddStop(trip.Stops[i]);
            //for (int i = swapIndex2 - 1; i > swapIndex1; i--)
            //    newTrip.AddStop(trip.Stops[i]);
            //for (int i = swapIndex2; i < trip.Stops.Count; i++)
            //    newTrip.AddStop(trip.Stops[i]);
            //truck.Days[locationD.Item2, locationD.Item3] = newTrip;
        }

        /// <summary>
        /// Calculates the potential cost a two opt mutation has
        /// </summary>
        /// <param name="locationD">Truck, day, trip</param>
        /// <param name="swapIndex1">Exclusive lower index</param>
        /// <param name="swapIndex2">Exclusive upper index</param>
        /// <returns>Returns the value the trip has after the two opt and a bool if the trip is valid</returns>
        private (double,bool) Check2Opt((int , int , int ) locationD, int swapIndex1, int swapIndex2)
        {
            return (0, false);
            /*
            // TODO: REDO
            var truck = locationD.Item1 == 0 ? Truck1 : Truck2;
            var trip = truck.Days[locationD.Item2, locationD.Item3];
            var othterTrip = truck.Days[locationD.Item2, locationD.Item3 == 0 ? 1 : 0];
            double tempValue = 30 * 60;
            bool valid = Valid;
            var tempOrder = OrderDict[trip.Stops[0]];
            tempValue += DistanceMatrix[287, tempOrder.MatrixId];
            tempValue += tempOrder.LedigingsDuur;
            //check this
            for(int i = 0; i < swapIndex1; i++)
            {
                tempOrder = OrderDict[trip.Stops[i]];
                var tempOrder2 = OrderDict[trip.Stops[i + 1]];
                tempValue += DistanceMatrix[tempOrder.MatrixId, tempOrder2.MatrixId];
                tempValue += tempOrder2.LedigingsDuur;
            }
            tempValue += DistanceMatrix[OrderDict[trip.Stops[swapIndex1]].MatrixId, OrderDict[trip.Stops[swapIndex2 - 1]].MatrixId];
            tempValue += OrderDict[trip.Stops[swapIndex2 - 1]].LedigingsDuur;
            for (int i = swapIndex2 - 1; i > swapIndex1 + 1; i--)
            {
                tempOrder = OrderDict[trip.Stops[i]];
                var tempOrder2 = OrderDict[trip.Stops[i - 1]];
                tempValue += DistanceMatrix[tempOrder.MatrixId, tempOrder2.MatrixId];
                tempValue += tempOrder2.LedigingsDuur;
            }
            tempValue += DistanceMatrix[OrderDict[trip.Stops[swapIndex1 + 1]].MatrixId, OrderDict[trip.Stops[swapIndex2]].MatrixId];
            tempValue += OrderDict[trip.Stops[swapIndex2]].LedigingsDuur;
            for (int i = swapIndex2; i < trip.Stops.Count - 1; i++)
            {
                tempOrder = OrderDict[trip.Stops[i]];
                var tempOrder2 = OrderDict[trip.Stops[i + 1]];
                tempValue += DistanceMatrix[tempOrder.MatrixId, tempOrder2.MatrixId];
                tempValue += tempOrder2.LedigingsDuur;
            }
            tempValue += DistanceMatrix[OrderDict[trip.Stops[trip.Stops.Count - 1]].MatrixId, 287];

            valid = tempValue + othterTrip.Duration < 12 * 60 * 60;

            return (tempValue, valid);
            */
        }

        /// <summary>
        /// Calculates potential costs after shift
        /// </summary>
        /// <param name="locations">A list of truck, day and trip where the order needs to ber removed and added</param>
        /// <param name="removeIndex">Index at which place an order needs to be remobed</param>
        /// <param name="addIndex">Index at which place an order needs to be added</param>
        /// <returns>Costs after shift</returns>
        private (double, bool) CheckShiftTrip(List<(int, int, int)> locations, int removeIndex, int addIndex)
        {
            //throw new NotImplementedException();
            double finalValue = Value;
            bool valid = Valid;
            

            var removeTruck = locations[0].Item1 == 0 ? Truck1 : Truck2;
            var trip1rem = removeTruck.Days[locations[0].Item2, 0];
            var trip2rem = removeTruck.Days[locations[0].Item2, 1];
            var removeTrip = removeTruck.Days[locations[0].Item2, locations[0].Item3];

            var addTruck = locations[1].Item1 == 0 ? Truck1 : Truck2;
            var trip1Add = addTruck.Days[locations[1].Item2, 0];
            var trip2Add = addTruck.Days[locations[1].Item2, 1];
            var addTrip = addTruck.Days[locations[1].Item2, locations[1].Item3];

            double diffRem = removeTrip.Duration;
            double diffAdd = addTrip.Duration;

            if (addTrip.Stops.Count < 2 || removeTrip.Stops.Count < 2)
                return (0, false);

            var prevNode = removeTruck.Days[locations[0].Item2, locations[0].Item3].UnsortedStops[removeIndex];

            int orderId = prevNode.Value;
            var order = OrderDict[orderId];

            int previous = prevNode.Previous != null ? OrderDict[prevNode.Previous.Value].MatrixId : 287;
            int current = order.MatrixId;
            int next = prevNode.Next != null ? OrderDict[prevNode.Next.Value].MatrixId : 287;

            diffRem -= DistanceMatrix[previous, current];
            diffRem -= DistanceMatrix[current, next];
            diffRem += DistanceMatrix[previous, next];

            diffRem -= order.LedigingsDuur;

            var nextNode = addTrip.UnsortedStops[addIndex];
            previous = nextNode.Previous != null ? OrderDict[nextNode.Previous.Value].MatrixId : 287;
            next = 287;

            if (addIndex < addTrip.Stops.Count )
                next = OrderDict[nextNode.Value].MatrixId;

            diffAdd -= DistanceMatrix[previous,next];
            diffAdd += DistanceMatrix[previous, current];
            diffAdd += DistanceMatrix[current, next];

            diffAdd += order.LedigingsDuur;
            
            if(locations[0].Item1 != locations[1].Item1 || locations[0].Item2 != locations[1].Item2 || locations[0].Item3 != locations[1].Item3)
            {
                if (removeTrip.Stops.Count == 1)
                    diffRem -= 1800;
                if (addTrip.Stops.Count == 0)
                    diffAdd += 1800;
            }
            finalValue += diffRem + diffAdd - removeTrip.Duration - addTrip.Duration;
            diffRem += locations[0].Item3 == 0 ? removeTruck.Days[locations[0].Item2, 1].Duration : removeTruck.Days[locations[0].Item2, 0].Duration;
            diffAdd += locations[1].Item3 == 0 ? addTruck.Days[locations[1].Item2, 1].Duration : addTruck.Days[locations[1].Item2, 0].Duration;
            if (diffAdd >= 12 * 60 * 60 || diffRem >= 12d * 60 * 60 ||
                addTrip.Weight + order.AantalContainers * order.Volume > 100000 || removeTrip.Weight - order.AantalContainers * order.Volume > 100000)
                valid = false;
            return (finalValue, valid);
        }

        /// <summary>
        /// Executes shift of order
        /// </summary>
        /// <param name="locations">A list of truck, day and trip where the order needs to ber removed and added</param>
        /// <param name="removeIndex">Index at which place an order needs to be removed</param>
        /// <param name="addIndex">Index at which place an order needs to be added</param>
        private void ShiftTrip(List<(int,int,int)> locations, int removeIndex, int addIndex)
        {
            var removeTruck = locations[0].Item1 == 0 ? Truck1 : Truck2;
            var addTruck = locations[1].Item1 == 0 ? Truck1 : Truck2;

            int orderId = removeTruck.Days[locations[0].Item2, locations[0].Item3].UnsortedStops[removeIndex].Value;
            var order = OrderDict[orderId];

            order.nodes[locations[0].Item2] = null;


            removeTruck.Days[locations[0].Item2, locations[0].Item3].DeleteStop(removeIndex);

            int locationIndex = order.Locations.IndexOf(locations[0]);
            order.Locations.RemoveAt(locationIndex);

            addTruck.Days[locations[1].Item2, locations[1].Item3].AddStop(orderId, addIndex);
            order.Locations.Add((locations[1].Item1, locations[1].Item2, locations[1].Item3));
            order.nodes[locations[1].Item2] = addTruck.Days[locations[1].Item2, locations[1].Item3].UnsortedStops[addTruck.Days[locations[1].Item2, locations[1].Item3].UnsortedStops.Count - 1];
        }

        /// <summary>
        /// Copies the solution
        /// </summary>
        /// <returns>Returns hard a copy of it self</returns>
        public Solution Copy()
        {
            indexError = false;
            foreach (var location in OrderDict.Values)
            {
                location.Locations = new List<(int, int, int)>();
                //location.nodes = new LinkedListNode<int>[5];
            }
            var res = new Solution();
            res.Penalty = Penalty;
            res.Truck1 = Truck1.Copy(0);
            res.Truck2 = Truck2.Copy(1);
            return res;
        }

        /// <summary>
        /// Calculate the potential new costs of an add mutation
        /// </summary>
        /// <param name="orderId">The order id that needs to be added</param>
        /// <param name="locations">Truck, Day, Trip</param>
        /// <returns>The new costs of the potential solution</returns>
        public (double, bool) CheckAddOrders(int orderId, List<(int, int, int)> locations)
        {
            var order = OrderDict[orderId];
            double finalValue = Value;
            finalValue -= order.Freq * order.LedigingsDuur * 3;

            if (order.Locations.Count > 0)
                return (Value, false);

            bool valid = Valid;

            foreach (var location in locations)
            {
                var truck = location.Item1 == 0 ? Truck1 : Truck2;
                var trip1 = truck.Days[location.Item2, 0];
                var trip2 = truck.Days[location.Item2, 1];
                var trip = location.Item3 == 0 ? trip1 : trip2;

                int previousLoc = 287;
                int nextLoc = 287;
                double newTime = trip.Duration;

                if (trip.Stops.Count == 0)
                    newTime += 1800;
                if (trip.Stops.Count > 0)
                    previousLoc = OrderDict[trip.Stops.Last.Value].MatrixId;

                newTime += DistanceMatrix[previousLoc, order.MatrixId];
                newTime += DistanceMatrix[order.MatrixId, nextLoc];
                newTime -= DistanceMatrix[previousLoc, nextLoc];

                newTime += order.LedigingsDuur;
                finalValue += newTime - trip.Duration;
                newTime += location.Item3 == 1 ? trip1.Duration : trip2.Duration;
                double temp = trip1.Duration + trip2.Duration + newTime;
                if (newTime >= 12d * 60 * 60 || trip.Weight + order.AantalContainers * order.Volume > 100000)
                {
                    valid = false;
                }
                
            }

            return (finalValue, valid);
        }

        /// <summary>
        /// Calculates the potential costs of adding orders on specific locations with specified index
        /// </summary>
        /// <param name="orderId">The orderId that needs to be added</param>
        /// <param name="locations">Truck, Day, Trip, Index</param>
        /// <returns>Returns potential cost</returns>
        public (double, bool) CheckAddOrders(int orderId, List<(int, int, int, int)> locations)
        {
            var order = OrderDict[orderId];
            double finalValue = Value;
            finalValue -= order.Freq * order.LedigingsDuur * 3;

            if (order.Locations.Count > 0)
                return (Value, false);

            bool valid = Valid;

            foreach (var location in locations)
            {
                int previousLoc;
                int nextLoc;
                double diff = 0;

                var truck = location.Item1 == 0 ? Truck1 : Truck2;
                var trip1 = truck.Days[location.Item2, 0];
                var trip2 = truck.Days[location.Item2, 1];
                var trip = location.Item3 == 0 ? trip1 : trip2;
                if (trip.Stops.Count == 0)
                {
                    diff = 1800;
                    previousLoc = 287;
                    nextLoc = 287;

                }
                else
                {
                    var node = trip.UnsortedStops[location.Item4];

                    previousLoc = node.Previous != null ? OrderDict[node.Previous.Value].MatrixId : 287;
                    nextLoc = OrderDict[node.Value].MatrixId;
                }

                diff += DistanceMatrix[previousLoc, order.MatrixId];
                diff += DistanceMatrix[order.MatrixId, nextLoc];
                diff -= DistanceMatrix[previousLoc, nextLoc];

                diff += order.LedigingsDuur;

                if (trip1.Duration + trip2.Duration + diff >= 12 * 60 * 60 || trip.Weight + order.AantalContainers * order.Volume > 100000)
                    valid = false;

                finalValue += diff;
            }

            return (finalValue, valid);
        }

        public (double, bool) CheckAddOrders(int orderId, List<(int, int, int, LinkedListNode<int>)> locations)
        {
            var order = OrderDict[orderId];
            double finalValue = Value;
            finalValue -= order.Freq * order.LedigingsDuur * 3;

            if (order.Locations.Count > 0)
                return (Value, false);

            bool valid = Valid;

            foreach (var location in locations)
            {
                int previousLoc;
                int nextLoc;
                double diff = 0;

                var truck = location.Item1 == 0 ? Truck1 : Truck2;
                var trip1 = truck.Days[location.Item2, 0];
                var trip2 = truck.Days[location.Item2, 1];
                var trip = location.Item3 == 0 ? trip1 : trip2;
                if (trip.Stops.Count == 0)
                {
                    diff = 1800;
                    previousLoc = 287;
                    nextLoc = 287;

                }
                else
                {
                    var node = location.Item4;

                    previousLoc = node.Previous != null ? OrderDict[node.Previous.Value].MatrixId : 287;
                    nextLoc = OrderDict[node.Value].MatrixId;
                }

                diff += DistanceMatrix[previousLoc, order.MatrixId];
                diff += DistanceMatrix[order.MatrixId, nextLoc];
                diff -= DistanceMatrix[previousLoc, nextLoc];

                diff += order.LedigingsDuur;

                if (trip1.Duration + trip2.Duration + diff >= 12 * 60 * 60 || trip.Weight + order.AantalContainers * order.Volume > 100000)
                    valid = false;

                finalValue += diff;
            }

            return (finalValue, valid);
        }

        /// <summary>
        /// Mutates the solution by adding an order
        /// </summary>
        /// <param name="orderId">The order that needs to be added</param>
        /// <param name="locations">The locations where the order needs to be added</param>
        public void AddOrders(int orderId, List<(int, int, int)> locations, bool intial = false)
        {
            var order = OrderDict[orderId];
            if (intial)
            {
                if (order.Locations.Count == order.Freq)
                    return;
                Penalty -= (order.Freq * order.LedigingsDuur * 3) / order.Freq;
            }
            else
            {
                if (order.Locations.Count > 0)
                    return;
                Penalty -= order.Freq * order.LedigingsDuur * 3;
            }

            foreach (var location in locations)
            {
                var truck = location.Item1 == 0 ? Truck1 : Truck2;

                truck.Days[location.Item2, location.Item3].AddStop(orderId);

                order.Locations.Add((location.Item1, location.Item2, location.Item3));
                order.nodes[location.Item2] = truck.Days[location.Item2, location.Item3].UnsortedStops[truck.Days[location.Item2, location.Item3].UnsortedStops.Count - 1];
            }            
        }

        public void AddOrders(int orderId, List<(int, int, int,int)> locations)
        {
            var order = OrderDict[orderId];

            if (order.Locations.Count > 0)
                return;

            foreach (var location in locations)
            {
                var truck = location.Item1 == 0 ? Truck1 : Truck2;

                truck.Days[location.Item2, location.Item3].AddStop(orderId, location.Item4);

                order.Locations.Add((location.Item1, location.Item2, location.Item3));
                order.nodes[location.Item2] = truck.Days[location.Item2, location.Item3].UnsortedStops[truck.Days[location.Item2, location.Item3].UnsortedStops.Count - 1];
            }

            Penalty -= order.Freq * order.LedigingsDuur * 3;
        }

        public void AddOrders(int orderId, List<(int, int, int, LinkedListNode<int>)> locations)
        {
            var order = OrderDict[orderId];

            if (order.Locations.Count > 0)
                return;

            foreach (var location in locations)
            {
                var truck = location.Item1 == 0 ? Truck1 : Truck2;
                if(location.Item4 != null)
                    truck.Days[location.Item2, location.Item3].AddStop(orderId, location.Item4);
                else
                    truck.Days[location.Item2, location.Item3].AddStop(orderId, rng.Next(truck.Days[location.Item2, location.Item3].Stops.Count));

                order.Locations.Add((location.Item1, location.Item2, location.Item3));
                order.nodes[location.Item2] = truck.Days[location.Item2, location.Item3].UnsortedStops[truck.Days[location.Item2, location.Item3].UnsortedStops.Count - 1];
            }

            Penalty -= order.Freq * order.LedigingsDuur * 3;
        }

        /// <summary>
        /// Calculate the potential new costs of removing an order
        /// </summary>
        /// <param name="orderId">The order that needs to be removed</param>
        /// <returns>The new costs of the potential solution</returns>
        public (double, bool) CheckRemoveOrder(int orderId)
        {
            var order = OrderDict[orderId];

            if (order.Locations.Count == 0)
                return (Value, false);

            bool valid = Valid;

            double finalValue = Value;
            finalValue += order.Freq * order.LedigingsDuur * 3;

            foreach(var location in order.Locations)
            {
                var truck = location.Item1 == 0 ? Truck1 : Truck2;
                var trip1 = truck.Days[location.Item2, 0];
                var trip2 = truck.Days[location.Item2, 1];
                var trip = location.Item3 == 0 ? trip1 : trip2;
                double diff = 0;

                var node = order.nodes[location.Item2] ;

                int previousLoc = node.Previous != null ? OrderDict[node.Previous.Value].MatrixId : 287;
                int nextLoc = node.Next != null ? OrderDict[node.Next.Value].MatrixId : 287;
                /*if (index == -1)
                {
                    indexError = true;
                    return (0, false);
                }*/

                diff += DistanceMatrix[previousLoc, nextLoc];
                diff -= DistanceMatrix[previousLoc, order.MatrixId];
                diff -= DistanceMatrix[order.MatrixId, nextLoc];

                diff -= order.LedigingsDuur;
                if (trip.Stops.Count == 1)
                    diff -= 1800;

                if (trip1.Duration + trip2.Duration + diff >= 12 * 60 * 60 || trip.Weight - order.AantalContainers * order.Volume > 100000)
                    valid = false;

                finalValue += diff;
            }

            return (finalValue, valid);
        }

        /// <summary>
        /// Removes the order from the solution
        /// </summary>
        /// <param name="orderId">The order that needs to be removed</param>
        public void RemoveOrders(int orderId)
        {
            var order = OrderDict[orderId];

            if (order.Locations.Count == 0)
                return;

            Penalty += order.Freq * order.LedigingsDuur * 3;

            foreach (var location in order.Locations)
            {
                var truck = location.Item1 == 0 ? Truck1 : Truck2;

                int index = truck.Days[location.Item2, location.Item3].UnsortedStops.FindIndex(x => x.Value == orderId);
                truck.Days[location.Item2, location.Item3].DeleteStop(index);
                
            }

            order.Locations = new List<(int, int, int)>();
            order.nodes = new LinkedListNode<int>[5];
        }

        /// <summary>
        /// Calculate the potential new costs of shifting two orders
        /// </summary>
        /// <param name="location">Which truck, which day and which trip</param>
        /// <param name="fromIndex">Location where the order is shifted from</param>
        /// <param name="toIndex">Location where the order is shifted to</param>
        /// <returns></returns>
        public (double, bool) CheckShiftOrder((int, int, int) location, int fromIndex, int toIndex)
        {
            double finalValue = Value;

            var truck = location.Item1 == 0 ? Truck1 : Truck2;
            var trip1 = truck.Days[location.Item2, 0];
            var trip2 = truck.Days[location.Item2, 1];
            var trip = location.Item3 == 0 ? trip1 : trip2;
            var fromNode = trip.UnsortedStops[fromIndex];

            if (fromIndex == toIndex)
                return (Value, false);


            int previousLoc = fromNode.Previous != null ? OrderDict[fromNode.Previous.Value].MatrixId : 287;
            int currentLoc = OrderDict[fromNode.Value].MatrixId;
            int nextLoc = fromNode.Next != null ? OrderDict[fromNode.Next.Value].MatrixId : 287;
            bool valid = Valid;
            double diff = 0;

            diff -= DistanceMatrix[previousLoc, currentLoc];
            diff -= DistanceMatrix[currentLoc, nextLoc];
            diff += DistanceMatrix[previousLoc, nextLoc];

            var toNode = trip.UnsortedStops[toIndex];


            previousLoc = 287;
            nextLoc = 287;

            // TODO curTruck.Days[daga, tripa].UnsortedStops[fromIndex].Next == curTruck.Days[daga, tripa].UnsortedStops[toIndex]
            //previousLoc = toNode.Previous != null ? OrderDict[toNode.Previous.Value].MatrixId : 287;
            //currentLoc = OrderDict[toNode.Value].MatrixId;
            //nextLoc = toNode.Next != null ? OrderDict[toNode.Next.Value].MatrixId : 287;

            if (toNode.Previous == fromNode)
            {
                previousLoc = OrderDict[toNode.Value].MatrixId;
                nextLoc = toNode.Next != null ? OrderDict[toNode.Next.Value].MatrixId : 287;
            }
            else
            {
                previousLoc = toNode.Previous != null ? OrderDict[toNode.Previous.Value].MatrixId : 287;
                nextLoc = OrderDict[toNode.Value].MatrixId;
            }

            diff -= DistanceMatrix[previousLoc, nextLoc];
            diff += DistanceMatrix[previousLoc, currentLoc];
            diff += DistanceMatrix[currentLoc, nextLoc];

            if (trip1.Duration + trip2.Duration + diff >= 12 * 60 * 60)
                valid = false;

            finalValue += diff;

            return (finalValue, valid);
        }

        /// <summary>
        /// Shifts the order in the solution
        /// </summary>
        /// <param name="location">Which truck, which day and which trip</param>
        /// <param name="fromIndex">Location where the order is shifted from</param>
        /// <param name="toIndex">Location where the order is shifted to</param>
        public void ShiftOrder((int, int, int) location, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return;
            var truck = location.Item1 == 0 ? Truck1 : Truck2;
            int orderId = truck.Days[location.Item2, location.Item3].UnsortedStops[fromIndex].Value;
            truck.Days[location.Item2, location.Item3].AddStop(orderId, toIndex);
            OrderDict[orderId].nodes[location.Item2] = truck.Days[location.Item2, location.Item3].UnsortedStops[truck.Days[location.Item2, location.Item3].UnsortedStops.Count - 1];
            /*if (fromIndex > toIndex)
                fromIndex += 1;*/

            truck.Days[location.Item2, location.Item3].DeleteStop(fromIndex);
        }

        /// <summary>
        /// Calculates the acceptation chance for simulated anealing
        /// </summary>
        /// <param name="oldValue">The previous value of the solution</param>
        /// <param name="newValue">The new value if the solution</param>
        /// <param name="T">The temperature</param>
        /// <returns>The acceptation chance</returns>
        public double AcceptationChance(double oldValue, double newValue, double T)
        {
            return Math.Exp((oldValue - newValue) / T);
        }
    }
}

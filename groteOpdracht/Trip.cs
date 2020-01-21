using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static groteOpdracht.LocalSearch;

namespace groteOpdracht
{
    class Trip
    {
        public LinkedList<int> Stops = new LinkedList<int>();
        public List<LinkedListNode<int>> UnsortedStops = new List<LinkedListNode<int>>();

        public double Duration = 0, Weight;
        /// <summary>
        /// Checks if the total weight is les than its capacity
        /// </summary>
        public bool Valid
        {
            get
            {
                return Weight <= 100000;
            }
        }

        /// <summary>
        /// Copies the trip
        /// </summary>
        /// <returns>Returns a hard copy of it self</returns>
        public Trip Copy(int truck, int dag, int trip)
        {
            var res = new Trip();
            res.Weight = Weight;
            res.Duration = Duration;
            var node = Stops.First;
            for(int i = 0; i < Stops.Count; i++)
            {
                res.Stops.AddLast(node.Value);
                res.UnsortedStops.Add(UnsortedStops[i]);

                OrderDict[node.Value].Locations.Add((truck,dag,trip, node));
                node = node.Next;
            }
            return res;
        }

        /// <summary>
        /// Adds a given order at the end of the stops list
        /// </summary>
        /// <param name="orderId">The order that needs to be added</param>
        public void AddStop(int orderId)
        {
            if (Stops.Count == 0)
                Duration = 1800;
            var order = OrderDict[orderId];
            int previousLoc = 287;
            if (Stops.Count != 0)
                previousLoc = OrderDict[Stops.Last.Value].MatrixId;

            Duration += DistanceMatrix[previousLoc, order.MatrixId];
            Duration += DistanceMatrix[order.MatrixId, 287];
            Duration -= DistanceMatrix[previousLoc, 287];

            Duration += order.LedigingsDuur;

            Weight += order.Volume * order.AantalContainers;

            Stops.AddLast(orderId);
            UnsortedStops.Add(Stops.Last);
        }

        /// <summary>
        /// Adds a given order to the stop list at a given index
        /// </summary>
        /// <param name="order">The order that needs to be added</param>
        /// <param name="index">The index at which place the order needs to be added</param>
        public void AddStop(int orderId, int index)
        {
            if(index == Stops.Count)
            {
                AddStop(orderId);
                return;
            }
            if (Stops.Count == 0)
                Duration = 1800;
            var order = OrderDict[orderId];

            int previousLoc = 287;
            int nextLoc = OrderDict[Stops[index]].MatrixId;

            if (index != 0)
                previousLoc = OrderDict[Stops[index - 1]].MatrixId;

            Duration += DistanceMatrix[previousLoc, order.MatrixId];
            Duration += DistanceMatrix[order.MatrixId, nextLoc];
            Duration -= DistanceMatrix[previousLoc, nextLoc];

            Duration += order.LedigingsDuur;

            Weight += order.Volume * order.AantalContainers;

            Stops.Insert(index, orderId);
        }

        /// <summary>
        /// Deletes the order at a specified index
        /// </summary>
        /// <param name="index">The index at which place an order needs to be removed</param>
        public void DeleteStop(int index)
        {
            if (Stops.Count == 0)
                return;

            var node = UnsortedStops[index];

            var order = OrderDict[node.Value];

            int previousLoc = node.Previous != null ? node.Previous.Value : 287;
            int nextLoc = node.Next != null ? node.Next.Value : 287;
            
            Duration += DistanceMatrix[previousLoc, nextLoc];
            Duration -= DistanceMatrix[previousLoc, order.MatrixId];
            Duration -= DistanceMatrix[order.MatrixId, nextLoc];

            Duration -= order.LedigingsDuur;

            Weight -= order.Volume * order.AantalContainers;

            

            Stops.RemoveAt(index);
            // TODO: proper remove
            UnsortedStops.RemoveAt(index);

            if (Stops.Count == 0)
                Duration = 0;
        }

    }
}

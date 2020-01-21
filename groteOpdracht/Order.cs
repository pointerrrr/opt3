using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace groteOpdracht
{
    class Order
    {
        public int Id, Freq, AantalContainers, Volume, MatrixId;
        public double LedigingsDuur;
        public string Plaats;

        public List<(int, int, int)> Locations = new List<(int, int, int)>();
    }
}

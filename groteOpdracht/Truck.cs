using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace groteOpdracht
{
    class Truck
    {
        public Trip[,] Days = new Trip[5,2];

        /// <summary>
        /// Returns total drivetime of a truck
        /// </summary>
        public double DriveTime
        {
            get
            {
                double res = 0;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        res += Days[i, j].Duration;
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Returns if a truck is valid
        /// </summary>
        public bool Valid
        {
            get
            {
                bool valid = true;
                for(int i = 0; i < 5 && valid; i++)
                {
                    double duration = 0;
                    for(int j = 0; j < 2 && valid; j++)
                    {
                        duration += Days[i,j].Duration;
                        valid &= Days[i,j].Valid && duration < 12 * 60 * 60;
                    }
                }
                return valid;
            }
        }

        public Truck()
        {
            for(int i = 0; i < 5; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    Days[i, j] = new Trip();
                }
            }
        }

        /// <summary>
        /// Copies the truck
        /// </summary>
        /// <returns>Returns a hard copy of it self</returns>
        public Truck Copy(int truck)
        {
            var res = new Truck();
            for(int i = 0; i < 5; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    res.Days[i,j] = Days[i,j].Copy(truck, i, j);
                }
            }
            return res;
        }
    }
}

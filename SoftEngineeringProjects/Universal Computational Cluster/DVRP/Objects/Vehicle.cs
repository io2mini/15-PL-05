using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP.Objects
{
    [Serializable]
    public class Vehicle
    {
        private Tuple<double, double> _location;
        private int _capacity;
        private int _speed;

        public Vehicle (Tuple<double, double> location, int capacity, int speed)
        {
            _location = location;
            _capacity = capacity;
            _speed = speed;
        }
    }
}

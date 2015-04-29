using System;

namespace DVRP.Objects
{
    [Serializable]
    public class Location
    {
        private readonly double _x;
        private readonly double _y;

        public Location(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// </summary>
        /// <param name="coordinate">Zmienna inicjalizująca obie współrzędne.</param>
        public Location(double coordinate)
            : this(coordinate, coordinate)
        {
        }

        /// <summary>
        ///     Konstruktor lokalizacji o współrzędnych (0,0).
        /// </summary>
        public Location()
            : this(0)
        {
        }

        /// <summary>
        ///     Współrzędna X.
        /// </summary>
        public double X
        {
            get { return _x; }
        }

        /// <summary>
        ///     Współrzędna Y.
        /// </summary>
        public double Y
        {
            get { return _y; }
        }

        /// <summary>
        ///     Euklidesowa funkcja odległości
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns>Odległość między l1 i l2.</returns>
        public static double operator |(Location l1, Location l2)
        {
            return DistanceBetween(l1, l2);
        }

        /// <summary>
        ///     Euklidesowa funkcja odległości
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns>Odległość między l1 i l2.</returns>
        public static double DistanceBetween(Location l1, Location l2)
        {
            return Math.Sqrt(Math.Pow(l1.X - l2.X, 2) + Math.Pow(l1.Y - l2.Y, 2));
        }

        /// <summary>
        ///     Euklidesowa funkcja odległości
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Zwraca odległość other od tego obiektu</returns>
        public double DistanceFrom(Location other)
        {
            return this | other;
        }
    }
}
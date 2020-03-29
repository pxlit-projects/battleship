using System;
using Battleship.Domain.GridDomain;

namespace Battleship.Domain
{
    //DO NOT TOUCH THIS FILE!

    /// <summary>
    /// Indicates a direction (e.g. North, SoutWest, ...)
    /// </summary>
    public class Direction
    {
        private static readonly Random RandomGenerator = new Random();

        public static Direction North = new Direction(0, -1);
        public static Direction NorthEast = new Direction(1, -1);
        public static Direction East = new Direction(1, 0);
        public static Direction SouthEast = new Direction(1, 1);
        public static Direction South = new Direction(0, 1);
        public static Direction SouthWest = new Direction(-1, 1);
        public static Direction West = new Direction(-1, 0);
        public static Direction NorthWest = new Direction(-1, -1);

        /// <summary>
        /// Array of all possible directions
        /// </summary>
        public static Direction[] AllDirections => new[] { North, East, South, West, NorthEast, SouthEast, SouthWest, NorthWest };

        /// <summary>
        /// Array of directions that are horizontal or vertical
        /// </summary>
        public static Direction[] BasicDirections => new[] { North, East, South, West };

        /// <summary>
        /// Horizontal direction.
        /// -1 = left
        /// 1 = right
        /// 0 = no horizontal direction 
        /// </summary>
        public int XStep { get; }

        /// <summary>
        /// Vertical direction.
        /// -1 = up
        /// 1 = down
        /// 0 = no vertical direction 
        /// </summary>
        public int YStep { get; }

        /// <summary>
        /// Returns the opposite direction. E.g. the opposite of North is South.
        /// </summary>
        public Direction Opposite => new Direction(XStep * -1, YStep * -1);

        private Direction(int xStep, int yStep)
        {
            XStep = xStep;
            YStep = yStep;
        }

        /// <summary>
        /// Returns a random direction.
        /// </summary>
        /// <param name="allowDiagonal">
        /// If true the direction can be diagonal.
        /// Default = false.
        /// </param>
        public static Direction CreateRandomly(bool allowDiagonal = false)
        {
            int index = RandomGenerator.Next(0, allowDiagonal ? 8 : 4);
            return AllDirections[index];
        }

        /// <summary>
        /// Get the direction when you go from coordinate 1 to coordinate 2
        /// </summary>
        public static Direction FromCoordinates(GridCoordinate coordinate1, GridCoordinate coordinate2)
        {
            int xStep = coordinate2.Column - coordinate1.Column;
            if (xStep != 0) xStep = xStep / Math.Abs(xStep);

            int yStep = coordinate2.Row - coordinate1.Row;
            if (yStep != 0) yStep = yStep / Math.Abs(yStep);

            return new Direction(xStep, yStep);
        }

        public override string ToString()
        {
            if (YStep == 0)
            {
                return XStep == 1 ? "East" : "West";
            }

            string direction = YStep == -1 ? "North" : "South";
            if (XStep == -1) direction += "West";
            if (XStep == 1) direction += "East";
            return direction;

        }

        #region Equality overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as Direction);
        }

        protected bool Equals(Direction other)
        {
            if (ReferenceEquals(other, null)) return false;
            return XStep == other.XStep && YStep == other.YStep;
        }

        public static bool operator ==(Direction a, Direction b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Direction a, Direction b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(XStep, YStep);
        }

        #endregion
    }
}
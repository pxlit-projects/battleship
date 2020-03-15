using System;
using Battleship.Domain.GridDomain;

namespace Battleship.Domain.FleetDomain
{
    /// <summary>
    /// There are 5 kind of ships in a battleship game.
    /// Each kind has a code, a name and a size. This class encapsulates this information.
    /// </summary>
    public class ShipKind
    {
        //IN THIS CLASS YOU SHOULD ONLY ALTER THE GenerateRandomSegmentCoordinates METHOD!

        public static ShipKind Carrier = new ShipKind("CAR", "Carrier", 5);
        public static ShipKind Battleship = new ShipKind("BS", "Battleship", 4);
        public static ShipKind Destroyer = new ShipKind("DS", "Destroyer", 3);
        public static ShipKind Submarine = new ShipKind("SM", "Submarine", 3);
        public static ShipKind PatrolBoat = new ShipKind("PB", "Patrol boat", 2);
        
        /// <summary>
        /// Array containing the 5 ship kinds.
        /// </summary>
        public static ShipKind[] All => new[]{Carrier, Battleship, Destroyer, Submarine, PatrolBoat};

        public string Code { get; }
        public string Name { get; }
        public int Size { get; }

        private ShipKind(string code, string name, int size)
        {
            Code = code;
            Name = name;
            Size = size;
        }

        public static ShipKind CreateFromCode(string shipCode)
        {
            switch (shipCode.ToUpper())
            {
                case "CAR":
                    return Carrier;
                case "BS":
                    return Battleship;
                case "DS":
                    return Destroyer;
                case "SM":
                    return Submarine;
                case "PB":
                    return PatrolBoat;
                default:
                    throw new ApplicationException($"The ship code {shipCode} does not exist.");
            }
        }


        /// <summary>
        /// Randomly generates an array of possible ship segment coordinates for the kind of ship.
        /// This method can be used to position a ship at random on a grid.
        /// </summary>
        /// <param name="gridSize">The coordinates will be within this grid size</param>
        /// <param name="allowDeformedShips">
        /// If false (=default), the coordinates will be horizontally or vertically aligned and will touch each other.
        /// If true, the coordinates will touch each other, but may possibly not be aligned (this is an EXTRA).
        /// </param>
        public GridCoordinate[] GenerateRandomSegmentCoordinates(int gridSize, bool allowDeformedShips = false)
        {
            throw new NotImplementedException("GenerateRandomSegmentCoordinates method of ShipKind class is not implemented");

            //Tip: use existing methods of the GridCoordinate and Direction classes.
        }

        #region Equality overrides
        //DO NOT TOUCH THIS METHODS IN THIS REGION!

        public override bool Equals(object obj)
        {
            return Equals(obj as ShipKind);
        }

        protected bool Equals(ShipKind other)
        {
            if (ReferenceEquals(other, null)) return false;
            return Code == other.Code;
        }

        public static bool operator ==(ShipKind a, ShipKind b)
        {
            if (ReferenceEquals(a,null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(ShipKind a, ShipKind b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        #endregion
    }
}
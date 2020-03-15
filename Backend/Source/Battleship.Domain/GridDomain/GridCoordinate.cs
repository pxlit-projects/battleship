using System;

namespace Battleship.Domain.GridDomain
{
    public class GridCoordinate
    {
        public int Row { get; }
        public int Column { get; }

        public GridCoordinate(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static GridCoordinate CreateRandom(int gridSize)
        {
            throw new NotImplementedException("CreateRandom method of GridCoordinate class is not implemented");
        }

        public bool IsOutOfBounds(int gridSize)
        {
            throw new NotImplementedException("IsOutOfBounds method of GridCoordinate class is not implemented");
        }

        public GridCoordinate GetNeighbor(Direction direction)
        {
            throw new NotImplementedException("GetNeighbor method of GridCoordinate class is not implemented");
        }

        public GridCoordinate GetOtherEnd(Direction direction, int distance)
        {
            throw new NotImplementedException("GetOtherEnd method of GridCoordinate class is not implemented");
        }

        #region overrides
        //DO NOT TOUCH THIS METHODS IN THIS REGION!

        public override string ToString()
        {
            return $"({Row},{Column})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GridCoordinate);
        }

        protected bool Equals(GridCoordinate other)
        {
            if (ReferenceEquals(other, null)) return false;
            return Row == other.Row && Column == other.Column;
        }

        public static bool operator ==(GridCoordinate a, GridCoordinate b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(GridCoordinate a, GridCoordinate b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        #endregion
    }

}
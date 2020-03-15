using System;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Domain.GridDomain
{
    public class GridSquare : IGridSquare
    {
        public GridSquareStatus Status { get; set; }

        public GridCoordinate Coordinate { get; }

        public int NumberOfBombs { get; private set; }

        /// <summary>
        /// When a grid square is hit by a bomb (HitByBomb method is called), the OnHitByBomb event will be invoked.
        /// The square being hit is the sender of the event.
        /// </summary>
        public event HitByBombHandler OnHitByBomb;

        public GridSquare(GridCoordinate coordinate)
        {
            throw new NotImplementedException("Constructor of GridSquare class is not implemented");
        }

        public void HitByBomb()
        {
            throw new NotImplementedException("HitByBomb method of GridSquare class is not implemented");
        }
    }

    public delegate void HitByBombHandler(IGridSquare sender);
}
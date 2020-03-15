using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Business.Models
{
    public class GridSquareInfo
    {
        /// <summary>
        /// A grid square can have 3 statuses:
        /// 0 = Untouched (not hit by any bombs),
        /// 1 = Miss (hit by at least one bomb, but not hosting a ship),
        /// 2 = Hit (hit by a bomb and hosting a ship
        /// </summary>
        public GridSquareStatus Status { get; }

        /// <summary>
        /// Number of bombs that hit the square.
        /// In a classic game this will normally be 0 or 1,
        /// but when ships can be moved, it can be opportune to shoot squares multiple times.
        /// </summary>
        public int NumberOfBombs { get; }

        public GridSquareInfo(IGridSquare square)
        {
            Status = square.Status;
            NumberOfBombs = square.NumberOfBombs;
        }
    }
}
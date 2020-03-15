using Battleship.Domain.FleetDomain;
using Battleship.Domain.GridDomain;

namespace Battleship.Business.Models.Contracts
{
    public interface IShipInfo
    {
        /// <summary>
        /// The coordinates of each segment of the ship.
        /// The length of this array will be equal to the size of the kind of ship
        /// (e.g. the coordinates of a Carrier will have a length of 5).
        /// </summary>
        GridCoordinate[] Coordinates { get; }

        /// <summary>
        /// The kind of ship (Carrier, Battleship, Destroyer, Submarine, Patrol boat).
        /// This determines the size, code and name of the ship.
        /// </summary>
        ShipKind Kind { get; }

        /// <summary>
        /// Indicates if all the segments of a ship are hit by a bomb.
        /// </summary>
        bool HasSunk { get; }
    }
}
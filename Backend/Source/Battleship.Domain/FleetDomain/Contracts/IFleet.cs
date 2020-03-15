using System.Collections.Generic;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Domain.FleetDomain.Contracts
{
    public interface IFleet
    {
        bool IsPositionedOnGrid { get; }
        Result TryMoveShipTo(ShipKind kind, GridCoordinate[] segmentCoordinates, IGrid grid);
        void RandomlyPositionOnGrid(IGrid grid, bool allowDeformedShips = false);
        IShip FindShipAtCoordinate(GridCoordinate coordinate);
        IList<IShip> GetAllShips();
        IList<IShip> GetSunkenShips();
    }
}
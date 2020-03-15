using System;
using System.Collections.Generic;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Domain.FleetDomain
{
    public class Fleet : IFleet
    {
        public bool IsPositionedOnGrid { get; }

        public Result TryMoveShipTo(ShipKind kind, GridCoordinate[] segmentCoordinates, IGrid grid)
        {
            throw new NotImplementedException("TryMoveShipTo method of Fleet class is not implemented");
        }

        public void RandomlyPositionOnGrid(IGrid grid, bool allowDeformedShips = false)
        {
            throw new NotImplementedException("RandomlyPositionOnGrid method of Fleet class is not implemented");
        }

        public IShip FindShipAtCoordinate(GridCoordinate coordinate)
        {
            throw new NotImplementedException("FindShipAtCoordinate method of Fleet class is not implemented");
        }

        public IList<IShip> GetAllShips()
        {
            throw new NotImplementedException("GetAllShips method of Fleet class is not implemented");
        }

        public IList<IShip> GetSunkenShips()
        {
            throw new NotImplementedException("GetSunkenShips method of Fleet class is not implemented");
        }
    }
}
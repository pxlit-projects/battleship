using System;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Domain.FleetDomain
{
    public class Ship : IShip
    {
        public IGridSquare[] Squares { get;}

        public ShipKind Kind { get; }

        public bool HasSunk { get; }

        public Ship(ShipKind kind)
        {
        }

        public void PositionOnGrid(IGridSquare[] squares)
        {
            throw new NotImplementedException("PositionOnGrid method of Ship class is not implemented");
        }

        public bool CanBeFoundAtCoordinate(GridCoordinate coordinate)
        {
            throw new NotImplementedException("CanBeFoundAtCoordinate method of Ship class is not implemented");
        }
    }

}
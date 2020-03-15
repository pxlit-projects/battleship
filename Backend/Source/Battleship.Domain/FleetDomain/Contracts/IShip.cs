using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Domain.FleetDomain.Contracts
{
    public interface IShip
    {
        IGridSquare[] Squares { get; }
        ShipKind Kind { get; }
        bool HasSunk { get; }
        void PositionOnGrid(IGridSquare[] squares);
        bool CanBeFoundAtCoordinate(GridCoordinate coordinate);
    }
}
using Battleship.Domain.FleetDomain;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Moq;

namespace Battleship.TestTools.Builders
{
    public class ShipBuilder
    {
        private readonly ShipKind _kind;
        private readonly Mock<IShip> _shipMock;

        public ShipBuilder(ShipKind kind)
        {
            _kind = kind;
            _shipMock = new Mock<IShip>();
            _shipMock.SetupGet(s => s.Squares).Returns(() => null);
            _shipMock.SetupGet(s => s.Kind).Returns(kind);
            _shipMock.SetupGet(s => s.HasSunk).Returns(false);
            _shipMock.Setup(s => s.CanBeFoundAtCoordinate(It.IsAny<GridCoordinate>())).Returns(false);
        }

        public ShipBuilder WithSquares(GridSquareStatus status = GridSquareStatus.Untouched)
        {
            _shipMock.SetupGet(s => s.Squares).Returns(new GridSquareArrayBuilder(_kind).WithStatus(status).BuildArray());
            return this;
        }

        public ShipBuilder WithSquares(IGridSquare[] squares)
        {
            _shipMock.SetupGet(s => s.Squares).Returns(squares);
            return this;
        }

        public ShipBuilder WithHasSunk(bool hasSunk)
        {
            _shipMock.SetupGet(s => s.HasSunk).Returns(hasSunk);
            return this;
        }

        public Mock<IShip> BuildMock()
        {
            return _shipMock;
        }

        public IShip Build()
        {
            return _shipMock.Object;
        }
    }
}
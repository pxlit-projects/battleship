using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Moq;

namespace Battleship.TestTools.Builders
{
    public class GridSquareBuilder
    {
        private readonly Mock<IGridSquare> _gridSquareMock;

        public GridSquareBuilder() : this(new GridCoordinateBuilder().Build())
        {

        }

        public GridSquareBuilder(GridCoordinate coordinate)
        {
            _gridSquareMock = new Mock<IGridSquare>();
            _gridSquareMock.SetupGet(s => s.Status).Returns(GridSquareStatus.Untouched);
            _gridSquareMock.SetupGet(s => s.Coordinate).Returns(coordinate);
            _gridSquareMock.SetupGet(s => s.NumberOfBombs).Returns(0);
            _gridSquareMock.Setup(g => g.HitByBomb()).Callback(() =>
            {
                _gridSquareMock.SetupGet(s => s.Status).Returns(GridSquareStatus.Miss);
                _gridSquareMock.SetupGet(s => s.NumberOfBombs).Returns(1);
            });
        }

        public GridSquareBuilder WithStatus(GridSquareStatus status)
        {
            _gridSquareMock.SetupGet(s => s.Status).Returns(status);
            if (status != GridSquareStatus.Untouched)
            {
                _gridSquareMock.SetupGet(s => s.NumberOfBombs).Returns(1);
            }
            return this;
        }

        public Mock<IGridSquare> BuildMock()
        {
            return _gridSquareMock;
        }

        public IGridSquare Build()
        {
            return _gridSquareMock.Object;
        }
    }
}
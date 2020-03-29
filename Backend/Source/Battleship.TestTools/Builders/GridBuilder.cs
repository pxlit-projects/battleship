using Battleship.Domain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Moq;

namespace Battleship.TestTools.Builders
{
    public class GridBuilder
    {
        private readonly int _size;
        private readonly Mock<IGrid> _gridMock;
        private readonly IGridSquare[,] _squares;

        private readonly GridSquareBuilder[,] _squareBuilders;

        public GridBuilder(int size = 10)
        {
            _size = size;
            _gridMock = new Mock<IGrid>();
            _gridMock.SetupGet(g => g.Size).Returns(size);

            _squareBuilders = new GridSquareBuilder[size, size];
            _squares = new IGridSquare[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    _squareBuilders[i, j] = new GridSquareBuilder(new GridCoordinate(i, j));
                    _squares[i, j] = _squareBuilders[i, j].Build();
                }
            }

            _gridMock.SetupGet(g => g.Squares).Returns(_squares);
            _gridMock.Setup(g => g.GetSquareAt(It.IsAny<GridCoordinate>())).Returns((GridCoordinate coordinate) =>
                _squares[coordinate.Row, coordinate.Column]);
        }

        public GridBuilder WithSquareStatus(GridCoordinate coordinate, GridSquareStatus status)
        {
            var squareBuilder = _squareBuilders[coordinate.Row, coordinate.Column];
            squareBuilder.WithStatus(status);

            return this;
        }

        public GridBuilder WithAllSquaresWithStatus(GridSquareStatus status)
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    var squareBuilder = _squareBuilders[i, j];
                    squareBuilder.WithStatus(status);
                }
            }
            return this;
        }


        public Mock<IGrid> BuildMock()
        {
            return _gridMock;
        }

        public IGrid Build()
        {
            return _gridMock.Object;
        }
    }
}
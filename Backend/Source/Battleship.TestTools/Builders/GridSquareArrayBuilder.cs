using System.Linq;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Moq;

namespace Battleship.TestTools.Builders
{
    public class GridSquareArrayBuilder
    {
        private readonly Mock<IGridSquare>[] _gridSquareMocks;

        public GridSquareArrayBuilder(ShipKind kind)
        {
            _gridSquareMocks = new Mock<IGridSquare>[kind.Size];
            for (int i = 0; i < kind.Size; i++)
            {
                _gridSquareMocks[i] = new GridSquareBuilder().BuildMock();
            }
        }

        public GridSquareArrayBuilder WithStatus(GridSquareStatus status)
        {
            for (int i = 0; i < _gridSquareMocks.Length; i++)
            {
                _gridSquareMocks[i] = new GridSquareBuilder().WithStatus(status).BuildMock();
            }
            return this;
        }

        public Mock<IGridSquare>[] BuildMockArray()
        {
            return _gridSquareMocks;
        }

        public IGridSquare[] BuildArray()
        {
            return _gridSquareMocks.Select(m => m.Object).ToArray();
        }
    }
}
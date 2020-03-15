using System;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.Domain.PlayerDomain;
using Battleship.Domain.PlayerDomain.Contracts;
using Moq;

namespace Battleship.TestTools.Builders
{
    public class PlayerBuilder
    {
        private static readonly Random RandomGenerator = new Random();

        private readonly Mock<IPlayer> _playerMock;

        public Mock<IGrid> GridMock { get; }
        public Mock<IFleet> FleetMock { get; }

        public PlayerBuilder()
        {
            _playerMock = new Mock<IPlayer>();
            _playerMock.SetupGet(p => p.Id).Returns(Guid.NewGuid());
            _playerMock.SetupGet(p => p.HasBombsLoaded).Returns(RandomGenerator.NextBool());
            GridMock = new Mock<IGrid>();
            GridMock.Setup(g => g.Shoot(It.IsAny<GridCoordinate>())).Returns((GridCoordinate c) =>
                new GridSquare(c) { Status = GridSquareStatus.Miss });
            _playerMock.SetupGet(p => p.Grid).Returns(GridMock.Object);
            FleetMock = new Mock<IFleet>();
            FleetMock.SetupGet(f => f.IsPositionedOnGrid).Returns(RandomGenerator.NextBool());
            _playerMock.SetupGet(p => p.Fleet).Returns(FleetMock.Object);
        }

        public PlayerBuilder WithFleetPositionedOnGrid(bool isFleetPositionedOnGrid)
        {
            FleetMock.SetupGet(f => f.IsPositionedOnGrid).Returns(isFleetPositionedOnGrid);
            return this;
        }

        public PlayerBuilder WithBombsLoaded(bool hasBombsLoaded)
        {
            _playerMock.SetupGet(p => p.HasBombsLoaded).Returns(hasBombsLoaded);
            return this;
        }

        public Mock<IPlayer> BuildMock()
        {
            return _playerMock;
        }

        public IPlayer Build()
        {
            return _playerMock.Object;
        }
    }
}
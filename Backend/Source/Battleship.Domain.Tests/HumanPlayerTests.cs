using Battleship.Domain.FleetDomain;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.PlayerDomain;
using Battleship.Domain.PlayerDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    public class HumanPlayerTests : TestBase
    {
        private HumanPlayer _humanPlayer;
        private User _user;
        private GameSettings _settings;

        [SetUp]
        public void Setup()
        {
            _user = new UserBuilder().WithId().Build();
            _settings = new GameSettings();
            _humanPlayer = new HumanPlayer(_user, _settings);
        }

        [MonitoredTest("Constructor - Should initialize properties, grid and fleet")]
        public void Constructor_ShouldInitializePropertiesGridAndFleet()
        {
            Assert.That(_humanPlayer.Id, Is.EqualTo(_user.Id), "The id of the player must be the id of the user.");
            Assert.That(_humanPlayer.HasBombsLoaded, Is.False, "A player should not have bombs loaded after construction.");
            Assert.That(_humanPlayer.NickName, Is.EqualTo(_user.NickName), "Nickname should be the nickname of the user.");
            Assert.That(_humanPlayer.Fleet, Is.Not.Null, "The fleet of the player must be an instance of Fleet.");
            Assert.That(_humanPlayer.Grid, Is.Not.Null, "The grid of the player must be an instance of Grid.");
            Assert.That(_humanPlayer.Grid.Size, Is.EqualTo(_settings.GridSize), "The size of the grid must match the GridSize in the settings.");
        }

        [MonitoredTest("ReloadBombs - Should result in HasBombsLoaded being true")]
        public void ReloadBombs_ShouldResultInHasBombsLoadedBeingTrue()
        {
            //Act
            _humanPlayer.ReloadBombs();

            //Assert
            Assert.That(_humanPlayer.HasBombsLoaded, Is.True);
        }

        [MonitoredTest("ShootAt - Should result in HasBombsLoaded being false")]
        public void ShootAt_ShouldResultInHasBombsLoadedBeingFalse()
        {
            //Arrange
            var playerBuilder = new PlayerBuilder();
            IPlayer opponent = playerBuilder.Build();
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();

            _humanPlayer.ReloadBombs();

            //Act
            _humanPlayer.ShootAt(opponent, coordinate);

            //Assert
            playerBuilder.GridMock.Verify(g => g.Shoot(coordinate), Times.Once,
                "The Shoot method of the opponent grid should have been called.");
            Assert.That(_humanPlayer.HasBombsLoaded, Is.False);
        }

        [MonitoredTest("ShootAt - Should return missed when the shot is a miss.")]
        public void ShootAt_ShouldReturnMissedWhenTheShotIsAMiss()
        {
            //Arrange
            var playerBuilder = new PlayerBuilder();
            IPlayer opponent = playerBuilder.Build();
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();

            _humanPlayer.ReloadBombs();

            //Act
            var result = _humanPlayer.ShootAt(opponent, coordinate);

            //Assert
            playerBuilder.GridMock.Verify(g => g.Shoot(coordinate), Times.Once,
                "The Shoot method of the opponent grid should have been called.");
            Assert.That(result.ShotFired, Is.True, "The result should indicate that the shot was fired.");
            Assert.That(result.Hit, Is.False, "The result should indicate that it was not a hit.");
        }

        [MonitoredTest("ShootAt - Should return hit when a ship is hit.")]
        public void ShootAt_ShouldReturnHitWhenAShipIsHit()
        {
            //Arrange
            var playerBuilder = new PlayerBuilder();
            playerBuilder.GridMock.Setup(g => g.Shoot(It.IsAny<GridCoordinate>())).Returns((GridCoordinate c) =>
                new GridSquare(c) { Status = GridSquareStatus.Hit });
            var kind = ShipKind.All.NextRandomElement();
            IShip hitShip = new ShipBuilder(kind).Build();
            playerBuilder.FleetMock.Setup(f => f.FindShipAtCoordinate(It.IsAny<GridCoordinate>())).Returns(hitShip);

            IPlayer opponent = playerBuilder.Build();
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();

            _humanPlayer.ReloadBombs();

            //Act
            var result = _humanPlayer.ShootAt(opponent, coordinate);

            //Assert
            playerBuilder.GridMock.Verify(g => g.Shoot(coordinate), Times.Once,
                "The Shoot method of the opponent grid should have been called.");
            Assert.That(result.ShotFired, Is.True, "The result should indicate that the shot was fired.");
            Assert.That(result.Hit, Is.True, "The result should indicate that it was a hit.");
            playerBuilder.FleetMock.Verify(f => f.FindShipAtCoordinate(coordinate), Times.Once,
                "Use the FindShipAtCoordinate of the opponent fleet to determine which kind of ship was hit.");
        }
    }
}
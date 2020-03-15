using System;
using System.Runtime.CompilerServices;
using Battleship.Business.Models.Contracts;
using Battleship.Business.Services;
using Battleship.Business.Services.Contracts;
using Battleship.Domain;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GameDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.Domain.PlayerDomain;
using Battleship.Domain.PlayerDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Business.Tests
{
    public class GameServiceTests : TestBase
    {
        private Mock<IGameFactory> _gameFactoryMock;
        private Mock<IGameRepository> _gameRepositoryMock;
        private Mock<IGameInfoFactory> _gameInfoFactoryMock;
        private GameService _service;

        [SetUp]
        public void Setup()
        {
            _gameFactoryMock = new Mock<IGameFactory>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _gameInfoFactoryMock = new Mock<IGameInfoFactory>();
            _service = new GameService(_gameFactoryMock.Object,
                _gameRepositoryMock.Object,
                _gameInfoFactoryMock.Object);
        }

        [MonitoredTest("CreateGameForUser - Creates a single player game using the GameFactory and adds it to the repository")]
        public void CreateGameForUser_CreatesASinglePlayerGameUsingTheGameFactoryAndAddsItToTheRepository()
        {
            //Arrange
            GameSettings settings = new GameSettings();
            User user = new UserBuilder().WithId().Build();

            var createdGameMock = new Mock<IGame>();
            IGame createdGame = createdGameMock.Object;

            _gameFactoryMock.Setup(factory => factory.CreateNewSinglePlayerGame(settings, user)).Returns(createdGame);
            _gameRepositoryMock.Setup(repo => repo.Add(createdGame));

            var createdGameInfoMock = new Mock<IGameInfo>();
            IGameInfo createdGameInfo = createdGameInfoMock.Object;
            _gameInfoFactoryMock.Setup(factory => factory.CreateFromGame(createdGame, user.Id)).Returns(createdGameInfo);

            //Act
            IGameInfo gameInfo = _service.CreateGameForUser(settings, user);

            //Assert
            Assert.That(gameInfo, Is.SameAs(createdGameInfo), "The IGame returned should be an instance created by the IGameInfoFactory.");
            _gameFactoryMock.Verify();
            _gameRepositoryMock.Verify();
            _gameInfoFactoryMock.Verify();
        }

        [MonitoredTest("GetGameInfoForPlayer - Retrieves the game from the repository and uses the GameInfoFactory to convert it")]
        public void GetGameInfoForPlayer_RetrievesTheGameFromTheRepositoryAndUsesTheGameInfoFactoryToConvertIt()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            var existingGameMock = new Mock<IGame>();
            IGame existingGame = existingGameMock.Object;

            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Returns(existingGame);

            var createdGameInfoMock = new Mock<IGameInfo>();
            IGameInfo createdGameInfo = createdGameInfoMock.Object;
            _gameInfoFactoryMock.Setup(factory => factory.CreateFromGame(existingGame, userId)).Returns(createdGameInfo);

            //Act
            IGameInfo gameInfo = _service.GetGameInfoForPlayer(gameId, userId);

            //Assert
            Assert.That(gameInfo, Is.SameAs(createdGameInfo), "The IGame returned should be an instance created by the IGameInfoFactory.");
            _gameRepositoryMock.Verify();
            _gameInfoFactoryMock.Verify();
        }

        [MonitoredTest("GetGameInfoForPlayer - Should not catch DataNotFoundExceptions")]
        public void GetGameInfoForPlayer_ShouldNotCatchDataNotFoundExceptions()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();

            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Throws<DataNotFoundException>();

            //Act + Assert
            Assert.That(() => _service.GetGameInfoForPlayer(gameId, userId), Throws.InstanceOf<DataNotFoundException>());
        }

        [MonitoredTest("PositionShipOnGrid - Retrieves the game from the repository, gets the player from the game and uses the fleet of the player to position the ship")]
        public void PositionShipOnGrid_RetrievesTheGameFromTheRepositoryThenGetsThePlayerAndUsesTheFleetOfThePlayerToPositionTheShip()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            ShipKind shipKind = ShipKind.All.NextRandomElement();
            GridCoordinate[] segmentCoordinates =
            {
                new GridCoordinateBuilder().Build(), 
                new GridCoordinateBuilder().Build()
            };

            var gridMock = new Mock<IGrid>();
            IGrid grid = gridMock.Object;

            Result expectedResult = Result.CreateSuccessResult();

            var fleetMock = new Mock<IFleet>();
            fleetMock.Setup(f => f.TryMoveShipTo(shipKind, segmentCoordinates, grid)).Returns(expectedResult);
            IFleet fleet = fleetMock.Object;

            var playerMock = new Mock<IPlayer>();
            IPlayer player = playerMock.Object;
            playerMock.SetupGet(p => p.Fleet).Returns(fleet);
            playerMock.SetupGet(p => p.Grid).Returns(grid);

            var existingGameMock = new Mock<IGame>();
            existingGameMock.Setup(g => g.GetPlayerById(userId)).Returns(player);
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Returns(existingGame);

            //Act
            var result = _service.PositionShipOnGrid(gameId, userId, shipKind, segmentCoordinates);

            //Assert
            Assert.That(result, Is.SameAs(expectedResult),
                "The Result returned should be an instance created by the TryMoveShipTo method of the Fleet of the Player.");
            _gameRepositoryMock.Verify();
            existingGameMock.Verify();
            fleetMock.Verify();
        }

        [MonitoredTest("PositionShipOnGrid - Should not catch ApplicationExceptions")]
        public void PositionShipOnGrid_ShouldNotCatchApplicationExceptions()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            ShipKind shipKind = ShipKind.All.NextRandomElement();
            GridCoordinate[] segmentCoordinates =
            {
                new GridCoordinateBuilder().Build(),
                new GridCoordinateBuilder().Build()
            };

            var gridMock = new Mock<IGrid>();
            IGrid grid = gridMock.Object;

            var fleetMock = new Mock<IFleet>();
            fleetMock.Setup(f => f.TryMoveShipTo(shipKind, segmentCoordinates, grid)).Throws<ApplicationException>();
            IFleet fleet = fleetMock.Object;

            var playerMock = new Mock<IPlayer>();
            IPlayer player = playerMock.Object;
            playerMock.SetupGet(p => p.Fleet).Returns(fleet);
            playerMock.SetupGet(p => p.Grid).Returns(grid);

            var existingGameMock = new Mock<IGame>();
            existingGameMock.Setup(g => g.GetPlayerById(userId)).Returns(player);
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Returns(existingGame);

            //Act
            Assert.That(() => _service.PositionShipOnGrid(gameId, userId, shipKind, segmentCoordinates), Throws.InstanceOf<ApplicationException>());
        }

        [MonitoredTest("PositionShipOnGrid - Should not catch DataNotFoundExceptions")]
        public void PositionShipOnGrid_ShouldNotCatchDataNotFoundExceptions()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            ShipKind shipKind = ShipKind.All.NextRandomElement();
            GridCoordinate[] segmentCoordinates =
            {
                new GridCoordinateBuilder().Build(),
                new GridCoordinateBuilder().Build()
            };

            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Throws<DataNotFoundException>();

            //Act
            Assert.That(() => _service.PositionShipOnGrid(gameId, userId, shipKind, segmentCoordinates), Throws.InstanceOf<DataNotFoundException>());
        }


        [MonitoredTest("ShootAtOpponent - Retrieves the game from the repository and uses it to shoot")]
        public void ShootAtOpponent_RetrievesTheGameFromTheRepositoryAndUsesItToShoot()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            GridCoordinate targetCoordinate = new GridCoordinate(1,1);

            var existingGameMock = new Mock<IGame>();
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Returns(existingGame);

            ShotResult expectedShotResult = RuntimeHelpers.GetUninitializedObject(typeof(ShotResult)) as ShotResult;
            existingGameMock.Setup(game => game.ShootAtOpponent(userId, targetCoordinate)).Returns(expectedShotResult);

            //Act
            var returnedShotResult = _service.ShootAtOpponent(gameId, userId, targetCoordinate);

            //Assert
            Assert.That(returnedShotResult, Is.SameAs(expectedShotResult),
                "The ShotResult returned should be an instance created by the ShootAtOpponent method of the Game.");
            _gameRepositoryMock.Verify();
            existingGameMock.Verify();
        }

        [MonitoredTest("ShootAtOpponent - Should not catch ApplicationExceptions")]
        public void ShootAtOpponent_ShouldNotCatchApplicationExceptions()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            GridCoordinate targetCoordinate = new GridCoordinate(1, 1);

            var existingGameMock = new Mock<IGame>();
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Returns(existingGame);

            existingGameMock.Setup(game => game.ShootAtOpponent(userId, targetCoordinate)).Throws<ApplicationException>();

            //Act + Assert
            Assert.That(() => _service.ShootAtOpponent(gameId, userId, targetCoordinate), Throws.InstanceOf<ApplicationException>());
        }

        [MonitoredTest("ShootAtOpponent - Should not catch DataNotFoundExceptions")]
        public void ShootAtOpponent_ShouldNotCatchDataNotFoundExceptions()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            GridCoordinate targetCoordinate = new GridCoordinate(1, 1);

            _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).Throws<DataNotFoundException>();

            //Act + Assert
            Assert.That(() => _service.ShootAtOpponent(gameId, userId, targetCoordinate), Throws.InstanceOf<DataNotFoundException>());
        }
    }
}
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
using Battleship.Domain.PlayerDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Business.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "GameService", @"Battleship.Business\Services\GameService.cs")]
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

            _gameFactoryMock.Setup(factory => factory.CreateNewSinglePlayerGame(It.IsAny<GameSettings>(), It.IsAny<User>())).Returns(createdGame);

            var createdGameInfoMock = new Mock<IGameInfo>();
            IGameInfo createdGameInfo = createdGameInfoMock.Object;
            _gameInfoFactoryMock.Setup(factory => factory.CreateFromGame(It.IsAny<IGame>(), It.IsAny<Guid>())).Returns(createdGameInfo);

            //Act
            IGameInfo gameInfo = _service.CreateGameForUser(settings, user);

            //Assert
            _gameFactoryMock.Verify(factory => factory.CreateNewSinglePlayerGame(settings, user), Times.Once,
                "The 'CreateNewSinglePlayerGame' method of the IGameFactory is not called correctly.");
            _gameRepositoryMock.Verify(repo => repo.Add(createdGame), Times.Once,
                "The 'Add' method of the IGameRepository is not called correctly. It should add the game created by the IGameFactory.");
            _gameInfoFactoryMock.Verify(factory => factory.CreateFromGame(createdGame, user.Id), Times.Once,
                "The 'CreateFromGame' method of the IGameInfoFactory is not called correctly. It should use the game created by the IGameFactory and the id of the user as playerId.");

            Assert.That(gameInfo, Is.SameAs(createdGameInfo), "The IGame returned should be an instance created by the IGameInfoFactory.");
        }

        [MonitoredTest("GetGameInfoForPlayer - Retrieves the game from the repository and uses the GameInfoFactory to convert it")]
        public void GetGameInfoForPlayer_RetrievesTheGameFromTheRepositoryAndUsesTheGameInfoFactoryToConvertIt()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            var existingGameMock = new Mock<IGame>();
            IGame existingGame = existingGameMock.Object;

            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(existingGame);

            var createdGameInfoMock = new Mock<IGameInfo>();
            IGameInfo createdGameInfo = createdGameInfoMock.Object;
            _gameInfoFactoryMock.Setup(factory => factory.CreateFromGame(It.IsAny<IGame>(), It.IsAny<Guid>())).Returns(createdGameInfo);

            //Act
            IGameInfo gameInfo = _service.GetGameInfoForPlayer(gameId, userId);

            //Assert
            _gameRepositoryMock.Verify(repo => repo.GetById(gameId), Times.Once,
                "The 'GetById' method of the IGameRepository is not called correctly.");
            _gameInfoFactoryMock.Verify(factory => factory.CreateFromGame(existingGame, userId), Times.Once,
                "The 'CreateFromGame' method of the IGameInfoFactory is not called correctly. " +
                "The game retrieved with the IGameRepository and the id of the user should be provided.");

            Assert.That(gameInfo, Is.SameAs(createdGameInfo), "The IGame returned should be an instance created by the IGameInfoFactory.");
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
            fleetMock.Setup(f => f.TryMoveShipTo(It.IsAny<ShipKind>(), It.IsAny<GridCoordinate[]>(), It.IsAny<IGrid>()))
                .Returns(expectedResult);
            IFleet fleet = fleetMock.Object;

            var playerMock = new Mock<IPlayer>();
            IPlayer player = playerMock.Object;
            playerMock.SetupGet(p => p.Fleet).Returns(fleet);
            playerMock.SetupGet(p => p.Grid).Returns(grid);

            var existingGameMock = new Mock<IGame>();
            existingGameMock.Setup(g => g.GetPlayerById(It.IsAny<Guid>())).Returns(player);
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(existingGame);

            //Act
            var result = _service.PositionShipOnGrid(gameId, userId, shipKind, segmentCoordinates);

            //Assert
            Assert.That(result, Is.SameAs(expectedResult),
                "The Result returned should be an instance created by the TryMoveShipTo method of the Fleet of the Player.");

            _gameRepositoryMock.Verify(repo => repo.GetById(gameId), Times.Once,
                "The 'GetById' method of the IGameRepository is not called correctly.");

            existingGameMock.Verify(g => g.GetPlayerById(userId), Times.Once,
                "The 'GetPlayerById' method of the game returned by the IGameRepository is not called correctly. " +
                "The userId should be provided.");

            playerMock.VerifyGet(p => p.Fleet, Times.AtLeastOnce,
                "The Fleet property of the player returned by the 'GetPlayerById' method of the game, should be used. " +
                "On this Fleet property the 'TryMoveShipTo' method can be called.");

            playerMock.VerifyGet(p => p.Grid, Times.AtLeastOnce,
                "The Grid property of the player returned by the 'GetPlayerById' method of the game, " +
                "should be used to pass as an argument to the 'TryMoveShipTo' of the Fleet of the player.");

            fleetMock.Verify(f => f.TryMoveShipTo(shipKind, segmentCoordinates, grid), Times.Once,
                "The 'TryMoveShipTo' method of the Fleet of the player returned by the 'GetPlayerById' method of the game, is not called correctly.");
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
            fleetMock.Setup(f => f.TryMoveShipTo(It.IsAny<ShipKind>(), It.IsAny<GridCoordinate[]>(), It.IsAny<IGrid>())).Throws<ApplicationException>();
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

            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Throws<DataNotFoundException>();

            //Act
            Assert.That(() => _service.PositionShipOnGrid(gameId, userId, shipKind, segmentCoordinates), Throws.InstanceOf<DataNotFoundException>());
        }

        [MonitoredTest("StartGame - Retrieves the game from the repository and starts it")]
        public void StartGame_RetrievesTheGameFromTheRepositoryAndStartsIt()
        {
            //Arrange
            IPlayer player = new PlayerBuilder().Build();
            IPlayer opponent = new PlayerBuilder().Build();
            Result expectedResult = Result.CreateSuccessResult();
            var existingGameMock = new GameBuilder().WithPlayers(player, opponent).BuildMock();
            existingGameMock.Setup(g => g.Start()).Returns(expectedResult);
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(existingGame);

            //Act
            Result result = _service.StartGame(existingGame.Id, player.Id);

            //Assert
            _gameRepositoryMock.Verify(repo => repo.GetById(existingGame.Id), Times.Once,
                "The 'GetById' method of the IGameRepository is not called correctly.");

            existingGameMock.Verify(g => g.Start(), Times.Once, "The Start method of the retrieved game should be called.");
            Assert.That(result, Is.SameAs(expectedResult),
                "The result of the Start method of the retrieved game should be returned.");
        }

        [MonitoredTest("StartGame - Should fail when the playerId does not match any of the players")]
        public void StartGame_ShouldFailWhenThePlayerIdDoesNotMatchAnyOfThePlayers()
        {
            //Arrange
            IPlayer player = new PlayerBuilder().Build();
            IPlayer opponent = new PlayerBuilder().Build();
            var existingGameMock = new GameBuilder().WithPlayers(player, opponent).BuildMock();
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(existingGame);

            Guid otherPlayerId = Guid.NewGuid();

            //Act
            Result result = _service.StartGame(existingGame.Id, otherPlayerId);

            //Assert
            _gameRepositoryMock.Verify(repo => repo.GetById(existingGame.Id), Times.Once,
                "The 'GetById' method of the IGameRepository is not called correctly.");
            Assert.That(result.IsFailure, Is.True, "A failure result should be returned.");

            existingGameMock.Verify(g => g.Start(), Times.Never,
                "The Start method of the retrieved game should not be called when the playerId does not match with one of the 2 players of the game.");
        }

        [MonitoredTest("ShootAtOpponent - Retrieves the game from the repository and uses it to shoot")]
        public void ShootAtOpponent_RetrievesTheGameFromTheRepositoryAndUsesItToShoot()
        {
            //Arrange
            Guid userId = Guid.NewGuid();
            Guid gameId = Guid.NewGuid();
            GridCoordinate targetCoordinate = new GridCoordinate(1, 1);

            var existingGameMock = new Mock<IGame>();
            IGame existingGame = existingGameMock.Object;
            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(existingGame);

            ShotResult expectedShotResult = RuntimeHelpers.GetUninitializedObject(typeof(ShotResult)) as ShotResult;
            existingGameMock.Setup(game => game.ShootAtOpponent(It.IsAny<Guid>(), It.IsAny<GridCoordinate>())).Returns(expectedShotResult);

            //Act
            var returnedShotResult = _service.ShootAtOpponent(gameId, userId, targetCoordinate);

            //Assert
            Assert.That(returnedShotResult, Is.SameAs(expectedShotResult),
                "The ShotResult returned should be an instance created by the ShootAtOpponent method of the Game.");

            _gameRepositoryMock.Verify(repo => repo.GetById(gameId), Times.Once,
                "The 'GetById' method of the IGameRepository is not called correctly.");

            existingGameMock.Verify(game => game.ShootAtOpponent(userId, targetCoordinate), Times.Once,
                "The 'ShootAtOpponent' method of the game returned by the IGameRepository is not called correctly. " +
                "The id of the shooter and the target coordinate should be provided.");
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
            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(existingGame);

            existingGameMock.Setup(game => game.ShootAtOpponent(It.IsAny<Guid>(), It.IsAny<GridCoordinate>())).Throws<ApplicationException>();

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

            _gameRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Throws<DataNotFoundException>();

            //Act + Assert
            Assert.That(() => _service.ShootAtOpponent(gameId, userId, targetCoordinate), Throws.InstanceOf<DataNotFoundException>());
        }
    }
}
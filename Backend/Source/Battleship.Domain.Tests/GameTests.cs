using System;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GameDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.PlayerDomain;
using Battleship.Domain.PlayerDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "Game", @"Battleship.Domain\GameDomain\Game.cs")]
    public class GameTests : TestBase
    {
        private Game _game;
        private PlayerBuilder _player1Builder;
        private IPlayer _player1;
        private PlayerBuilder _player2Builder;
        private IPlayer _player2;
        private GameSettings _settings;


        [SetUp]
        public void Setup()
        {
            _settings = new GameSettingsBuilder().Build();
            _player1Builder = new PlayerBuilder().WithFleetPositionedOnGrid(true);
            _player1 = _player1Builder.Build();
            _player2Builder = new PlayerBuilder().WithFleetPositionedOnGrid(true);
            _player2 = _player2Builder.Build();
            _game = new Game(_settings, _player1, _player2);
        }

        [MonitoredTest("GameFactory - CreateNewSinglePlayerGame - Creates a game with a human and computer player with bombs loaded for the human")]
        public void GameFactory_CreateNewSinglePlayerGame_CreatesAGameWithHumanAndAComputer_WithBombsLoadedForTheHuman()
        {
            //Arrange
            GameFactory factory = new GameFactory();
            GameSettings settings = new GameSettings();
            User user = new UserBuilder().WithId().Build();

            //Act
            IGame game = factory.CreateNewSinglePlayerGame(settings, user);

            //Assert
            Assert.That(game, Is.Not.Null, "No instance of a class that implements IGame is returned.");
            Assert.That(game.Id, Is.Not.EqualTo(Guid.Empty), "The Id of the game must be a new Guid (Guid.NewGuid();).");
            Assert.That(game.Settings, Is.SameAs(settings),
                "The Settings of the game must be a reference to the same Settings object that was passed in as parameter.");

            Assert.That(game.Player1, Is.TypeOf<HumanPlayer>(), "Player 1 must be a HumanPlayer.");
            Assert.That(game.Player1.Id, Is.EqualTo(user.Id), "The Id of Player1 must be the Id of the User that was passed in as parameter.");
            Assert.That(game.Player1.HasBombsLoaded, Is.False, "No player should have bombs loaded after a game is created. " +
                                                               "Bombs are loaded when a game is started.");

            Assert.That(game.Player2, Is.TypeOf<ComputerPlayer>(), "Player 2 must be a ComputerPlayer.");
            Assert.That(game.Player2.Id, Is.Not.EqualTo(Guid.Empty), "The Id of Player2 must be a new Guid (Guid.NewGuid();).");
            Assert.That(game.Player2.HasBombsLoaded, Is.False, "No player should have bombs loaded after a game is created. " +
                                                               "Bombs are loaded when a game is started.");
        }

        [MonitoredTest("GetPlayerById - Returns the player with the matching id")]
        public void GetPlayerById_ReturnsThePlayerWithTheMatchingId()
        {
            Assert.That(_game.GetPlayerById(_player1.Id), Is.SameAs(_player1));
            Assert.That(_game.GetPlayerById(_player2.Id), Is.SameAs(_player2));
        }

        [MonitoredTest("GetOpponent - Returns the other player")]
        public void GetOpponent_ReturnsTheOtherPlayer()
        {
            Assert.That(_game.GetOpponent(_player1), Is.SameAs(_player2));
            Assert.That(_game.GetOpponent(_player2), Is.SameAs(_player1));
        }

        [MonitoredTest("Start - Should mark the game as started and load the bombs for player 1")]
        public void Start_ShouldMarkTheGameAsStartedAndLoadTheBombsForPlayer1()
        {
            //Act
            var result = _game.Start();

            //Assert
            Assert.That(result.IsSuccess, Is.True, "A success result should be returned.");
            Assert.That(_game.IsStarted, Is.True, "Game is not marked as started.");
            Assert.That(_game.Player1.HasBombsLoaded, Is.True, "Bombs for player 1 are not loaded.");
        }

        [MonitoredTest("Start - Should fail when the fleet of player 1 is not positioned")]
        public void Start_ShouldFailWhenTheFleetOfPlayer1IsNotPositioned()
        {
            //Arrange
            _player1Builder.WithFleetPositionedOnGrid(false);

            //Act
            var result = _game.Start();

            //Assert
            Assert.That(result.IsFailure, Is.True, "A failure result should be returned.");
            Assert.That(_game.IsStarted, Is.False, "Game should not be marked as started.");
        }

        [MonitoredTest("Start - Should fail when the fleet of player 2 is not positioned")]
        public void Start_ShouldFailWhenTheFleetOfPlayer2IsNotPositioned()
        {
            //Arrange
            _player2Builder.WithFleetPositionedOnGrid(false);

            //Act
            var result = _game.Start();

            //Assert
            Assert.That(result.IsFailure, Is.True, "A failure result should be returned.");
            Assert.That(_game.IsStarted, Is.False, "Game should not be marked as started.");
        }

        [MonitoredTest("ShootAtOpponent - Should use the ShootAt method of the player")]
        public void ShootAtOpponent_ShouldUseTheShootAtMethodOfThePlayer()
        {
            //Arrange
            GridCoordinate targetCoordinate = new GridCoordinateBuilder().Build();
            _player1Builder.WithBombsLoaded(true);

            ShotResult expectedShotResult = ShotResult.CreateMissed();
            Mock<IPlayer> player1Mock = _player1Builder.BuildMock();
            player1Mock.Setup(p => p.ShootAt(It.IsAny<IPlayer>(), It.IsAny<GridCoordinate>()))
                .Returns(expectedShotResult);

            _game.Start();

            //Act
            ShotResult result = _game.ShootAtOpponent(_player1.Id, targetCoordinate);

            player1Mock.Verify(p => p.ShootAt(_player2, targetCoordinate), Times.Once,
                "The ShootAt method of the player that shoots is not called correctly.");
            Assert.That(result, Is.SameAs(expectedShotResult), "The ShotResult returned by the ShootAt method should be returned.");
        }

        [MonitoredTest("ShootAtOpponent - Should return a misfire when the game is not started yet")]
        public void ShootAtOpponent_ShouldReturnAMisfireWhenTheGameIsNotStartedYet()
        {
            //Arrange
            GridCoordinate targetCoordinate = new GridCoordinateBuilder().Build();
            _player1Builder.WithBombsLoaded(true);
            Mock<IPlayer> player1Mock = _player1Builder.BuildMock();

            //Act
            ShotResult result = _game.ShootAtOpponent(_player2.Id, targetCoordinate);

            player1Mock.Verify(p => p.ShootAt(It.IsAny<IPlayer>(), It.IsAny<GridCoordinate>()), Times.Never,
                "The ShootAt method of the player that shoots should not be called when the game has not started yet.");
            Assert.That(result.ShotFired, Is.False, "The ShotResult should indicate that no shot is fired.");
            Assert.That(result.MisfireReason, Is.Not.Empty, "The ShotResult should contain a misfire reason.");
        }

        [MonitoredTest("ShootAtOpponent - Should return a misfire when no bombs are loaded")]
        public void ShootAtOpponent_ShouldReturnAMisfireWhenNoBombsAreLoaded()
        {
            //Arrange
            GridCoordinate targetCoordinate = new GridCoordinateBuilder().Build();
            _player2Builder.WithBombsLoaded(false);
            Mock<IPlayer> player2Mock = _player2Builder.BuildMock();

            _game.Start();

            //Act
            ShotResult result = _game.ShootAtOpponent(_player2.Id, targetCoordinate);

            player2Mock.Verify(p => p.ShootAt(It.IsAny<IPlayer>(), It.IsAny<GridCoordinate>()), Times.Never,
                "The ShootAt method of the player that shoots should not be called when no bombs are loaded.");
            Assert.That(result.ShotFired, Is.False, "The ShotResult should indicate that no shot is fired.");
            Assert.That(result.MisfireReason, Is.Not.Empty, "The ShotResult should contain a misfire reason.");
        }

        [MonitoredTest("ShootAtOpponent - Should let the Computer shoot when all bombs are shot")]
        public void ShootAtOpponent_ShouldLetTheComputerShootWhenAllBombsAreShot()
        {
            //Arrange
            var shootingStrategyMock = new Mock<IShootingStrategy>();
            IPlayer computerPlayer = new ComputerPlayer(_settings, shootingStrategyMock.Object);

            _player2Builder.WithBombsLoaded(true);
            IPlayer humanPlayer = _player2;

            _game = new Game(_settings, computerPlayer, humanPlayer);
            _game.Start();

            GridCoordinate targetCoordinate = new GridCoordinateBuilder().Build();

            Mock<IPlayer> humanPlayerMock = _player2Builder.BuildMock();
            ShotResult expectedShotResult = ShotResult.CreateMissed();
            humanPlayerMock.Setup(p => p.ShootAt(It.IsAny<IPlayer>(), It.IsAny<GridCoordinate>()))
                .Returns(() =>
                {
                    _player2Builder.WithBombsLoaded(false);
                    return expectedShotResult;
                });

            //Act
            ShotResult result = _game.ShootAtOpponent(_player2.Id, targetCoordinate);

            humanPlayerMock.Verify(p => p.ShootAt(computerPlayer, targetCoordinate), Times.Once,
                "The ShootAt method of the human player is not called correctly.");

            shootingStrategyMock.Verify(s => s.DetermineTargetCoordinate(), Times.AtLeast(1),
                "The computer player did not shoot. You must call the ShootAutomatically method of the computer player. " +
                "The ShootAutomatically method should in turn call the DetermineTargetCoordinate method of the shooting strategy of the computer. " +
                "This test fails because no call to the DetermineTargetCoordinate method is detected.");

            humanPlayerMock.Verify(p => p.ReloadBombs(), Times.Once,
                "The ReloadBombs method of the human player should be called after the computer is done with shooting.");

            Assert.That(result, Is.SameAs(expectedShotResult), "The ShotResult returned by the ShootAt method (of the human player) should be returned.");
        }
    }
}
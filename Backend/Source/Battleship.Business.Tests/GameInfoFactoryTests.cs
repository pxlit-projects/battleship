using System.Collections.Generic;
using Battleship.Business.Models;
using Battleship.Business.Models.Contracts;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GameDomain.Contracts;
using Battleship.Domain.PlayerDomain;
using Battleship.Domain.PlayerDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Business.Tests
{
    public class GameInfoFactoryTests : TestBase
    {
        private Mock<IGridInfoFactory> _gridInfoFactoryMock;
        private Mock<IShipInfoFactory> _shipInfoFactoryMock;

        private GameInfoFactory _factory;

        [SetUp]
        public void Setup()
        {
            _gridInfoFactoryMock = new Mock<IGridInfoFactory>();
            _shipInfoFactoryMock = new Mock<IShipInfoFactory>();
            _factory = new GameInfoFactory(_gridInfoFactoryMock.Object, _shipInfoFactoryMock.Object);
        }

        [MonitoredTest("CreateFromGame - Uses a game to get the info from the perspective of the player")]
        public void CreateFromGame_UsesAGameToGetTheInfoFromThePerspectiveOfThePlayer()
        {
            //Arrange
            var settings = new GameSettingsBuilder().WithMustReportSunkenShip(true).Build();
            Mock<IGame> gameMock = new GameBuilder().WithSettings(settings).BuildMock();
            IGame game = gameMock.Object;

            IGridInfo expectedPlayerGridInfo = new Mock<IGridInfo>().Object;
            IGridInfo expectedOpponentGridInfo = new Mock<IGridInfo>().Object;
            _gridInfoFactoryMock.Setup(f => f.CreateFromGrid(game.Player1.Grid)).Returns(expectedPlayerGridInfo);
            _gridInfoFactoryMock.Setup(f => f.CreateFromGrid(game.Player2.Grid)).Returns(expectedOpponentGridInfo);

            IList<IShipInfo> expectedPlayerShipInfos = new List<IShipInfo>();
            _shipInfoFactoryMock.Setup(f => f.CreateMultipleFromFleet(game.Player1.Fleet)).Returns(expectedPlayerShipInfos);
            IList<IShipInfo> expectedOpponentShipInfos = new List<IShipInfo>();
            _shipInfoFactoryMock.Setup(f => f.CreateMultipleFromSunkenShipsOfFleet(game.Player2.Fleet)).Returns(expectedOpponentShipInfos);

            //Act
            IGameInfo gameInfo = _factory.CreateFromGame(game, game.Player1.Id);

            //Assert
            Assert.That(gameInfo, Is.Not.Null, "No instance of a class that implements IGame is returned.");
            Assert.That(gameInfo.Id, Is.EqualTo(game.Id), "The Id should be the Id of the game.");

            gameMock.Verify(g => g.GetPlayerById(game.Player1.Id), Times.Once,
                "The player should be retrieved using the GetPlayerById method of the game correctly.");

            Assert.That(gameInfo.IsReadyToStart,
                Is.EqualTo(game.Player1.Fleet.IsPositionedOnGrid && game.Player2.Fleet.IsPositionedOnGrid),
                "IsReadyToStart must be true when both the player fleet as the opponent fleet is positioned on the grid.");

            Assert.That(gameInfo.HasBombsLoaded, Is.EqualTo(game.Player1.HasBombsLoaded), "HasBombsLoaded should be equal to HasBombsLoaded of the player");
            Assert.That(gameInfo.OwnGrid, Is.SameAs(expectedPlayerGridInfo),
                "The OwnGrid should be the instance returned by the IGridInfoFactory. " +
                "The IGridInfoFactory should create the info from the Grid of the player.");
            Assert.That(gameInfo.OwnShips, Is.SameAs(expectedPlayerShipInfos),
                "The OwnShips should be the list returned by the IShipInfoFactory. " +
                "The IShipInfoFactory should create the list from all the ships of the player.");

            gameMock.Verify(g => g.GetOpponent(game.Player1), Times.Once,
                "The opponent should be retrieved using the GetOpponent method of the game correctly.");

            Assert.That(gameInfo.OpponentGrid, Is.SameAs(expectedOpponentGridInfo),
                "The OpponentGrid should be the instance returned by the IGridInfoFactory. " +
                "The IGridInfoFactory should create the info from the Grid of the opponent.");
            Assert.That(gameInfo.SunkenOpponentShips, Is.SameAs(expectedOpponentShipInfos),
                "The SunkenOpponentShips should be the list returned by the IShipInfoFactory. " +
                "The IShipInfoFactory should create the list from all the sunken ships of the opponent.");
        }

        [Test]
        public void EXTRA_CreateFromGame_ShouldNotShowSunkenShipsOfOpponentIfSunkenShipsDoNotNeedToBeReported()
        {
            //Arrange
            IPlayer player = new PlayerBuilder().WithFleetPositionedOnGrid(true).Build();
            IPlayer opponent = new PlayerBuilder().WithFleetPositionedOnGrid(true).Build();
            var settings = new GameSettingsBuilder().WithMustReportSunkenShip(false).Build();
            var game = new GameBuilder().WithPlayers(player, opponent).WithSettings(settings).Build();

            IList<IShipInfo> opponentShipInfos = new List<IShipInfo>
            {
                new Mock<IShipInfo>().Object
            };
            _shipInfoFactoryMock.Setup(f => f.CreateMultipleFromSunkenShipsOfFleet(It.IsAny<Fleet>())).Returns(opponentShipInfos);

            //Act
            IGameInfo gameInfo = _factory.CreateFromGame(game, player.Id);

            //Assert
            Assert.That(gameInfo, Is.Not.Null, "No instance of a class that implements IGame is returned.");

            Assert.That(gameInfo.SunkenOpponentShips, Is.Not.Null, "SunkenOpponentShips must be an empty list.");
            Assert.That(gameInfo.SunkenOpponentShips.Count, Is.Zero, "SunkenOpponentShips must be an empty list.");

            _shipInfoFactoryMock.Verify(f => f.CreateMultipleFromSunkenShipsOfFleet(It.IsAny<Fleet>()), Times.Never,
                "CreateMultipleFromSunkenShipsOfFleet should not be called when sunken ships do not need to be reported.");
        }
    }
}
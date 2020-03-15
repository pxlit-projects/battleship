using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Battleship.Api.Controllers;
using Battleship.Api.Models;
using Battleship.Api.Tests.Builders;
using Battleship.Business.Models.Contracts;
using Battleship.Business.Services.Contracts;
using Battleship.Domain;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Battleship.Api.Tests
{
    [TestFixture]
    public class GameControllerTests : TestBase
    {
        private GameController _controller;
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IGameService> _gameServiceMock;
        private User _loggedInUser;

        [SetUp]
        public void Setup()
        {
            _gameServiceMock = new Mock<IGameService>();

            var userStoreMock = new Mock<IUserStore<User>>();
            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var errorsMock = new Mock<IdentityErrorDescriber>();
            var loggerMock = new Mock<ILogger<UserManager<User>>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                passwordHasherMock.Object,
                null,
                null,
                lookupNormalizerMock.Object,
                errorsMock.Object,
                null,
                loggerMock.Object);

            _controller = new GameController(_gameServiceMock.Object, _userManagerMock.Object);

            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
            var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
            context.HttpContext.User = userClaimsPrincipal;
            _controller.ControllerContext = context;

            _loggedInUser = new UserBuilder().WithId().Build();
            _userManagerMock.Setup(manager => manager.GetUserAsync(userClaimsPrincipal))
                .ReturnsAsync(_loggedInUser);
        }

        [MonitoredTest("CreateNewSinglePlayerGame - Should create a single player game for the provided game settings")]
        public void CreateNewSinglePlayerGame_WithGameSettingsProvided_ShouldCreateASinglePlayerGameAndReturnIt()
        {
            //Arrange
            GameSettings settings = new GameSettingsBuilder().Build();

            Guid createdGameId = Guid.NewGuid();
            var createdGameInfoMock = new Mock<IGameInfo>();
            createdGameInfoMock.SetupGet(gi => gi.Id).Returns(createdGameId);

            _gameServiceMock.Setup(service => service.CreateGameForUser(settings, _loggedInUser)).Returns(createdGameInfoMock.Object);


            //Act
            var result = _controller.CreateNewSinglePlayerGame(settings).Result as CreatedAtActionResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'CreatedAtActionResult' should be returned.");

            _userManagerMock.Verify();
            _gameServiceMock.Verify();

            Assert.That(result.ActionName, Is.EqualTo(nameof(GameController.GetGameInfo)), "The 'CreatedAtActionResult' does not refer to the right action.");
            Assert.That(result.RouteValues["id"], Is.EqualTo(createdGameId), "The 'CreatedAtActionResult' does not refer to the right game id.");
            Assert.That(result.Value, Is.EqualTo(createdGameInfoMock.Object), "The 'CreatedAtActionResult' does not hold the correct gameInfo object.");
        }

        [MonitoredTest("CreateNewSinglePlayerGame - Should create a single player game with default game settings if not settings are provided")]
        public void CreateNewSinglePlayerGame_NoSettingsProvided_ShouldCreateASinglePlayerGameWithDefaultSettingsAndReturnIt()
        {
            //Arrange
            Guid createdGameId = Guid.NewGuid();
            var createdGameInfoMock = new Mock<IGameInfo>();
            createdGameInfoMock.SetupGet(gi => gi.Id).Returns(createdGameId);

            _gameServiceMock.Setup(service => service.CreateGameForUser(It.IsAny<GameSettings>(), _loggedInUser)).Returns(createdGameInfoMock.Object);

            //Act
            var result = _controller.CreateNewSinglePlayerGame().Result as CreatedAtActionResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'CreatedAtActionResult' should be returned.");

            _userManagerMock.Verify();
            _gameServiceMock.Verify(
                service => service.CreateGameForUser(It.Is<GameSettings>(settings => AreDefaultSettings(settings)),
                    _loggedInUser), Times.Once, "The settings used are not default settings.");

            Assert.That(result.ActionName, Is.EqualTo(nameof(GameController.GetGameInfo)), "The 'CreatedAtActionResult' does not refer to the right action.");
            Assert.That(result.RouteValues["id"], Is.EqualTo(createdGameId), "The 'CreatedAtActionResult' does not refer to the right game id.");
            Assert.That(result.Value, Is.SameAs(createdGameInfoMock.Object), "The 'CreatedAtActionResult' does not hold the correct gameInfo object.");
        }

        [MonitoredTest("CreateNewSinglePlayerGame - Should return bad request when an ApplicationException occurs")]
        public void CreateNewSinglePlayerGame_ShouldReturnBadRequestWhenAnApplicationExceptionOccurs()
        {
            //Arrange
            _gameServiceMock.Setup(service =>
                    service.CreateGameForUser(It.IsAny<GameSettings>(), It.IsAny<User>()))
                .Throws<ApplicationException>();

            //Act
            var result = _controller.CreateNewSinglePlayerGame().Result as BadRequestObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'BadRequestObjectResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
            var serializableError = result.Value as SerializableError;
            Assert.IsNotNull(serializableError, "The 'BadRequestObjectResult' should contain a ModelState error collection");
            Assert.That(serializableError.Count, Is.EqualTo(1), "The 'BadRequestObjectResult' should exactly one error");
        }

        [MonitoredTest("GetGameInfo - Should retrieve the game info from the game service")]
        public void GetGameInfo_ShouldRetrieveGameInfoFromService()
        {
            //Arrange
            Guid existingGameId = Guid.NewGuid();
            var existingGameInfoMock = new Mock<IGameInfo>();
            existingGameInfoMock.SetupGet(gi => gi.Id).Returns(existingGameId);

            _gameServiceMock.Setup(service => service.GetGameInfoForPlayer(existingGameId, _loggedInUser.Id)).Returns(existingGameInfoMock.Object);

            //Act
            var result = _controller.GetGameInfo(existingGameId).Result as OkObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");

            _userManagerMock.Verify();
            _gameServiceMock.Verify();

            Assert.That(result.Value, Is.SameAs(existingGameInfoMock.Object),
                "The 'OkObjectResult' does not hold the gameInfo object returned by the game service.");
        }

        [MonitoredTest("GetGameInfo - Should return not found when the game does not exist")]
        public void GetGameInfo_ShouldReturnNotFountWhenGameDoesNotExist()
        {
            //Arrange
            _gameServiceMock.Setup(service => service.GetGameInfoForPlayer(It.IsAny<Guid>(), It.IsAny<Guid>())).Throws<DataNotFoundException>();

            //Act
            var result = _controller.GetGameInfo(Guid.NewGuid()).Result as NotFoundResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'NotFoundResult' should be returned.");

            _userManagerMock.Verify();
            _gameServiceMock.Verify();
        }

        [MonitoredTest("StartGame - Should use game service to start a game")]
        public void StartGame_ShouldUseServiceToStartAGame()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            Result expectedResult = Result.CreateSuccessResult();
            _gameServiceMock.Setup(service => service.StartGame(gameId, _loggedInUser.Id)).Returns(expectedResult);

            //Act
            var result = _controller.StartGame(gameId).Result as OkObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
            Assert.That(result.Value, Is.SameAs(expectedResult));
        }

        [MonitoredTest("StartGame - Should return NotFound when the game does not exist")]
        public void StartGame_ShouldReturnNotFoundWhenGameDoesNotExist()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            Result expectedResult = Result.CreateSuccessResult();
            _gameServiceMock.Setup(service => service.StartGame(It.IsAny<Guid>(), It.IsAny<Guid>())).Throws<DataNotFoundException>();

            //Act
            var result = _controller.StartGame(gameId).Result as NotFoundResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'NotFoundResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
        }

        [MonitoredTest("PositionShipOnGrid - Should use game service to position the ship")]
        public void PositionShipOnGrid_ShouldUseServiceToPositionTheShip()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            ShipPositioningModel model = new ShipPositioningModelBuilder().Build();

            Result expectedResult = Result.CreateSuccessResult();
            _gameServiceMock.Setup(service =>
                    service.PositionShipOnGrid(gameId, _loggedInUser.Id,
                        It.Is<ShipKind>(kind => kind.Code == model.ShipCode),
                        It.Is<GridCoordinate[]>(ca => ca.Length == model.SegmentCoordinates.Length && 
                                                      ca.All(c => model.SegmentCoordinates.Any(mc =>
                                                              c.Row == mc.Row && c.Column == mc.Column)))))
                .Returns(expectedResult);

            //Act
            var result = _controller.PositionShipOnGrid(gameId, model).Result as OkObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
            Assert.That(result.Value, Is.SameAs(expectedResult));
        }

        [MonitoredTest("PositionShipOnGrid - Should return bad request when game does not exist")]
        public void PositionShipOnGrid_ShouldReturnBadRequestWhenGameDoesNotExist()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            ShipPositioningModel model = new ShipPositioningModelBuilder().Build();

            _gameServiceMock.Setup(service =>
                    service.PositionShipOnGrid(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ShipKind>(), It.IsAny<GridCoordinate[]>()))
                .Throws<DataNotFoundException>();

            ActAndAssertPositionShipOnGridShouldReturnBadRequest(gameId, model);
        }

        [MonitoredTest("PositionShipOnGrid - Should return bad request when an ApplicationException occurs")]
        public void PositionShipOnGrid_ShouldReturnBadRequestWhenAnApplicationExceptionOccurs()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            ShipPositioningModel model = new ShipPositioningModelBuilder().Build();

            _gameServiceMock.Setup(service =>
                    service.PositionShipOnGrid(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ShipKind>(), It.IsAny<GridCoordinate[]>()))
                .Throws<ApplicationException>();

            ActAndAssertPositionShipOnGridShouldReturnBadRequest(gameId, model);
        }

        [MonitoredTest("ShootAtOpponent - Should use game service to register the shot")]
        public void ShootAtOpponent_ShouldUseServiceToRegisterTheShot()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            GridCoordinateModel coordinateModel = new GridCoordinateModelBuilder().Build();

            ShotResult expectedShotResult = ShotResult.CreateMissed();
            _gameServiceMock.Setup(service =>
                    service.ShootAtOpponent(gameId, _loggedInUser.Id,
                        It.Is<GridCoordinate>(
                            gc => gc.Row == coordinateModel.Row && gc.Column == coordinateModel.Column)))
                .Returns(expectedShotResult);

            //Act
            var result = _controller.ShootAtOpponent(gameId, coordinateModel).Result as OkObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
            Assert.That(result.Value, Is.SameAs(expectedShotResult));
        }

        [MonitoredTest("ShootAtOpponent - Should return bad request when game does not exist")]
        public void ShootAtOpponent_ShouldReturnBadRequestWhenGameDoesNotExist()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            GridCoordinateModel coordinateModel = new GridCoordinateModelBuilder().Build();

            _gameServiceMock.Setup(service =>
                    service.ShootAtOpponent(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<GridCoordinate>()))
                .Throws<DataNotFoundException>();

            ActAndAssertShootAtOpponentShouldReturnBadRequest(gameId, coordinateModel);
        }

        [MonitoredTest("ShootAtOpponent - Should return bad request when an ApplicationException occurs")]
        public void ShootAtOpponent_ShouldReturnBadRequestWhenAnApplicationExceptionOccurs()
        {
            //Arrange
            Guid gameId = Guid.NewGuid();
            GridCoordinateModel coordinateModel = new GridCoordinateModelBuilder().Build();

            _gameServiceMock.Setup(service =>
                    service.ShootAtOpponent(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<GridCoordinate>()))
                .Throws<ApplicationException>();

            ActAndAssertShootAtOpponentShouldReturnBadRequest(gameId, coordinateModel);
        }

        private void ActAndAssertShootAtOpponentShouldReturnBadRequest(Guid gameId, GridCoordinateModel coordinateModel)
        {
            //Act
            var result = _controller.ShootAtOpponent(gameId, coordinateModel).Result as BadRequestObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'BadRequestObjectResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
            var serializableError = result.Value as SerializableError;
            Assert.IsNotNull(serializableError, "The 'BadRequestObjectResult' should contain a ModelState error collection");
            Assert.That(serializableError.Count, Is.EqualTo(1), "The 'BadRequestObjectResult' should exactly one error");
        }

        private void ActAndAssertPositionShipOnGridShouldReturnBadRequest(Guid gameId, ShipPositioningModel model)
        {
            //Act
            var result = _controller.PositionShipOnGrid(gameId, model).Result as BadRequestObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null, "An instance of 'BadRequestObjectResult' should be returned.");
            _userManagerMock.Verify();
            _gameServiceMock.Verify();
            var serializableError = result.Value as SerializableError;
            Assert.IsNotNull(serializableError, "The 'BadRequestObjectResult' should contain a ModelState error collection");
            Assert.That(serializableError.Count, Is.EqualTo(1), "The 'BadRequestObjectResult' should exactly one error");
        }

        private bool AreDefaultSettings(GameSettings settings)
        {
            GameSettings defaultSettings = new GameSettings();
            if (defaultSettings.CanMoveUndamagedShipsDuringGame != settings.CanMoveUndamagedShipsDuringGame)
            {
                return false;
            }
            if (defaultSettings.MustReportSunkenShip != settings.MustReportSunkenShip) return false;
            if (defaultSettings.AllowDeformedShips != settings.AllowDeformedShips) return false;
            if (defaultSettings.NumberOfTurnsBeforeAShipCanBeMoved != settings.NumberOfTurnsBeforeAShipCanBeMoved)
            {
                return false;
            }
            if (defaultSettings.Mode != settings.Mode) return false;
            if (defaultSettings.GridSize != settings.GridSize) return false;

            return true;
        }
    }
}
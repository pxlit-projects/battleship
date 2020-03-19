using System;
using System.Linq;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.PlayerDomain;
using Battleship.Domain.PlayerDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using Guts.Client.Shared.TestTools;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "ComputerPlayer", @"Battleship.Domain\PlayerDomain\ComputerPlayer.cs")]
    public class ComputerPlayerTests : TestBase
    {
        private ComputerPlayer _computerPlayer;
        private GameSettings _settings;
        private Mock<IShootingStrategy> _shootingStrategyMock;

        [SetUp]
        public void Setup()
        {
            _settings = new GameSettings();
            _shootingStrategyMock = new Mock<IShootingStrategy>();
            _computerPlayer = new ComputerPlayer(_settings, _shootingStrategyMock.Object);
        }

        [MonitoredTest("Constructor - Should initialize properties, grid and fleet")]
        public void Constructor_ShouldInitializePropertiesGridAndFleet()
        {
            Assert.That(_computerPlayer.Id, Is.Not.EqualTo(Guid.Empty), "The id of the player must be a new unique Guid.");
            Assert.That(_computerPlayer.HasBombsLoaded, Is.False, "A player should not have bombs loaded after construction.");
            Assert.That(_computerPlayer.NickName, Is.EqualTo("Computer"), "Nickname should be equal to 'Computer'.");
            Assert.That(_computerPlayer.Grid, Is.Not.Null, "The grid of the player must be an instance of Grid.");
            Assert.That(_computerPlayer.Grid.Size, Is.EqualTo(_settings.GridSize), "The size of the grid must match the GridSize in the settings.");
            Assert.That(_computerPlayer.Fleet, Is.Not.Null, "The fleet of the player must be an instance of Fleet.");
        }

        [MonitoredTest("Constructor - Should randomly position the fleet on the grid")]
        public void Constructor_ShouldRandomlyPositionTheFleetOnTheGrid()
        {
            Assert.That(_computerPlayer.Fleet, Is.Not.Null, "The fleet of the player must be an instance of Fleet.");
            Assert.That(_computerPlayer.Fleet.IsPositionedOnGrid, Is.True, "The fleet should be positioned on the grid.");

            var code = Solution.Current.GetFileContent(@"BattleShip.Domain\PLayerDomain\ComputerPlayer.cs");
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var constructor = root
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault();

            Assert.That(constructor, Is.Not.Null, "Could not find a constructor");

            var body = constructor.Body.ToString();

            Assert.That(body, Contains.Substring("Fleet.RandomlyPositionOnGrid(").IgnoreCase,
                "You must call the RandomlyPositionOnGrid on the fleet of the player.");
        }

        [MonitoredTest("ShootAutomatically - Should use the shooting strategy to shoot")]
        public void ShootAutomatically_ShouldUseTheShootingStrategyToShoot()
        {
            //Arrange
            _computerPlayer.ReloadBombs(); 

            var playerBuilder = new PlayerBuilder();
            IPlayer opponent = playerBuilder.Build();

            GridCoordinate determinedCoordinate = new GridCoordinateBuilder().Build();

            _shootingStrategyMock.Setup(ss => ss.DetermineTargetCoordinate()).Returns(determinedCoordinate);

            //Act
            _computerPlayer.ShootAutomatically(opponent);

            //Assert
            _shootingStrategyMock.Verify(ss => ss.DetermineTargetCoordinate(),
                "Use the DetermineTargetCoordinate method of the shooting strategy.");

            var opponentGridMock = playerBuilder.GridMock;
            opponentGridMock.Verify(g => g.Shoot(determinedCoordinate), Times.AtLeastOnce,
                "Use the ShootAt method to shoot at the opponent on the coordinate determined by the strategy.");

            _shootingStrategyMock.Verify(ss => ss.RegisterShotResult(It.IsAny<ShotResult>()),
                "After shooting, the result should be registered with the shooting strategy.");
        }
    }
}
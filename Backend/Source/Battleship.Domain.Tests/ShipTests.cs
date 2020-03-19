using System.Linq;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "Ship", @"Battleship.Domain\FleetDomain\Ship.cs")]
    public class ShipTests : TestBase
    {
        private ShipKind _kind;
        private Ship _ship;

        [SetUp]
        public void Setup()
        {
            _kind = ShipKind.All.NextRandomElement();
            _ship = new Ship(_kind);
        }

        [MonitoredTest("Constructor - Should set kind and leave squares null")]
        public void Constructor_ShouldSetKindAndLeaveSquaresNull()
        {
            //Assert
            Assert.That(_ship.Kind, Is.SameAs(_kind));
            Assert.That(_ship.Squares, Is.Null);
        }

        [MonitoredTest("PositionOnGrid - Should set set squares")]
        public void PositionOnGrid_ShouldSetSquares()
        {
            //Arrange
            IGridSquare[] squares = new GridSquareArrayBuilder(_kind).BuildArray();

            //Act
            _ship.PositionOnGrid(squares);

            //Assert
            Assert.That(_ship.Squares, Is.SameAs(squares));
        }

        [MonitoredTest("PositionOnGrid - Should add a handler for when a Square is hit")]
        public void PositionOnGrid_ShouldAddAHandlerForWhenASquareIsHit()
        {
            //Arrange
            var gridSquareArrayBuilder = new GridSquareArrayBuilder(_kind);
            Mock<IGridSquare>[] squareMocks = gridSquareArrayBuilder.BuildMockArray();
            Mock<IGridSquare> squareToHitMock = squareMocks.NextRandomElement();
            IGridSquare squareToHit = squareToHitMock.Object;
            squareToHitMock.Setup(g => g.HitByBomb()).Callback(() =>
            {
                squareToHitMock.Raise(s => s.OnHitByBomb += null, squareToHitMock.Object);
            });

            //Act
            _ship.PositionOnGrid(gridSquareArrayBuilder.BuildArray());
            squareToHit.HitByBomb();

            //Assert
            squareToHitMock.VerifySet(s => s.Status = GridSquareStatus.Hit, Times.Once,
                "When a GridSquare is hit and the square is occupied by a ship, " +
                "the ship should handle the OnHitByBomb event of the GridSquare and set its status to Hit.");
        }

        [MonitoredTest("PositionOnGrid - Should remove the handler for squares that are not occupied anymore")]
        public void PositionOnGrid_ShouldRemoveHandlerForSquaresThatAreNotOccupiedAnymore()
        {
            //Arrange
            var originalGridSquareArrayBuilder = new GridSquareArrayBuilder(_kind);
            Mock<IGridSquare>[] originalSquareMocks = originalGridSquareArrayBuilder.BuildMockArray();
            foreach (var squareMock in originalSquareMocks)
            {
                squareMock.Setup(g => g.HitByBomb()).Callback(() =>
                {
                    squareMock.Raise(s => s.OnHitByBomb += null, squareMock.Object);
                });
            }
            _ship.PositionOnGrid(originalGridSquareArrayBuilder.BuildArray());

            //Act
            _ship.PositionOnGrid(new GridSquareArrayBuilder(_kind).BuildArray());

            //Assert
            foreach (var squareMock in originalSquareMocks)
            {
                squareMock.Object.HitByBomb();
                squareMock.VerifySet(s => s.Status = It.IsAny<GridSquareStatus>(), Times.Never,
                    "When a GridSquare is hit and the square is not occupied by a ship anymore, " +
                    "the ship should not handle the OnHitByBomb event of the GridSquare anymore. " +
                    "Use the '-=' operator on the 'OnHitByBomb' event to remove the event handler.");
            }
        }

        [MonitoredTest("CanBeFoundAtCoordinate - Should return false when no Squares are set")]
        public void CanBeFoundAtCoordinate_ShouldReturnFalseWhenNoSquaresAreSet()
        {
            //Arrange
            GridCoordinate someCoordinate = new GridCoordinateBuilder().Build();

            //Act
            bool result = _ship.CanBeFoundAtCoordinate(someCoordinate);

            //Assert
            Assert.That(result, Is.False);
        }

        [MonitoredTest("CanBeFoundAtCoordinate - Should return true when one of the segment Squares matches")]
        public void CanBeFoundAtCoordinate_ShouldReturnTrueWhenOneOfTheSegmentSquaresMatches()
        {
            //Arrange
            IGridSquare[] squares = new GridSquareArrayBuilder(_kind).BuildArray();
            _ship.PositionOnGrid(squares);

            //Act + Assert
            foreach (var gridSquare in squares)
            {
                bool result = _ship.CanBeFoundAtCoordinate(gridSquare.Coordinate);
                Assert.That(result, Is.True, () =>
                {
                    string segmentCoordinates = squares.Select(s => s.Coordinate).ToArray().Print();
                    return $"Expected ship with segment coordinates {segmentCoordinates} to be found at {gridSquare.Coordinate}";
                });
            }
        }

        [MonitoredTest("CanBeFoundAtCoordinate - Should return false when none of the segment Squares match")]
        public void CanBeFoundAtCoordinate_ShouldReturnFalseWhenNoneOfTheSegmentSquaresMatch()
        {
            //Arrange
            IGridSquare[] squares = new GridSquareArrayBuilder(_kind).BuildArray();
            _ship.PositionOnGrid(squares);

            GridCoordinate noneShipCoordinate = new GridCoordinateBuilder().Build();
            while (squares.Any(s => s.Coordinate == noneShipCoordinate))
            {
                noneShipCoordinate = new GridCoordinateBuilder().Build();
            }

            //Act
            bool result = _ship.CanBeFoundAtCoordinate(noneShipCoordinate);

            //Assert
            Assert.That(result, Is.False, () =>
            {
                string segmentCoordinates = squares.Select(s => s.Coordinate).ToArray().Print();
                return $"Expected ship with segment coordinates {segmentCoordinates} not to be found at {noneShipCoordinate}";
            });
        }

        [MonitoredTest("HasSunk - Should return false when ship is not positioned yet")]
        public void HasSunk_ShouldReturnFalseWhenShipIsNotPositionedYet()
        {
            Assert.That(_ship.HasSunk, Is.False);
        }

        [MonitoredTest("HasSunk - Should return true when all Squares are hit")]
        public void HasSunk_ShouldReturnTrueWhenAllSquaresAreHit()
        {
            //Arrange
            IGridSquare[] squares = new GridSquareArrayBuilder(_kind).WithStatus(GridSquareStatus.Hit).BuildArray();
            _ship.PositionOnGrid(squares);

            //Act + Assert
            Assert.That(_ship.HasSunk, Is.True);
        }

        [MonitoredTest("HasSunk - Should return false when not all Squares are hit")]
        public void HasSunk_ShouldReturnFalseWhenNotAllSquaresAreHit()
        {
            //Arrange
            var gridSquareArrayBuilder = new GridSquareArrayBuilder(_kind).WithStatus(GridSquareStatus.Hit);
            Mock<IGridSquare>[] squareMocks = gridSquareArrayBuilder.BuildMockArray();
            Mock<IGridSquare> untouchedSquareMock = squareMocks.NextRandomElement();
            untouchedSquareMock.SetupGet(s => s.Status).Returns(GridSquareStatus.Untouched);
            IGridSquare[] squares = gridSquareArrayBuilder.BuildArray();
            _ship.PositionOnGrid(squares);

            //Act + Assert
            Assert.That(_ship.HasSunk, Is.False);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
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
    [ProjectComponentTestFixture("1TINProject", "Battleship", "Fleet", @"Battleship.Domain\FleetDomain\Fleet.cs")]
    public class FleetTests : TestBase
    {
        private Fleet _fleet;
        private Dictionary<ShipKind, IShip> _internalDictionary;
        private Mock<IGrid> _gridMock;

        [SetUp]
        public void Setup()
        {
            _fleet = new Fleet();
            if (_fleet.HasPrivateField<Dictionary<ShipKind, IShip>>())
            {
                _internalDictionary = _fleet.GetPrivateFieldValue<Dictionary<ShipKind, IShip>>();
            }
            _gridMock = new GridBuilder().BuildMock();
        }

        [MonitoredTest("Constructor - Should fill a dictionary with 5 different ships")]
        public void Constructor_ShouldFillADictionaryWith5DifferentShips()
        {
            AssertContains5Ships();
        }

        [MonitoredTest("IsPositionedOnGrid - Should return true when all ships are positioned on a grid")]
        public void IsPositionedOnGrid_ShouldReturnTrueWhenAllShipsArePositionedOnAGrid()
        {
            AssertContains5Ships();

            //Arrange
            ReplaceShipsWithMocks();

            //Act + Assert
            Assert.That(_fleet.IsPositionedOnGrid, Is.True,
                "When the Squares property of all ships is not null, IsPositionedOnGrid should be true.");
        }

        [MonitoredTest("IsPositionedOnGrid - Should return false when one of the ships is not positioned on a grid")]
        public void IsPositionedOnGrid_ShouldReturnFalseWhenOneOfTheShipsIsNotPositionedOnAGrid()
        {
            AssertContains5Ships();

            //Arrange
            Mock<IShip>[] shipMocks = ReplaceShipsWithMocks();

            var unPositionedShipMock = shipMocks.NextRandomElement();
            unPositionedShipMock.SetupGet(s => s.Squares).Returns(() => null);
            
            //Act + Assert
            Assert.That(_fleet.IsPositionedOnGrid, Is.False,
                "When the Squares property of one of the ships is null, IsPositionedOnGrid should be false.");
        }

        [MonitoredTest("FindShipAtCoordinate - Should return the ship that occupies the coordinate")]
        public void FindShipAtCoordinate_ShouldReturnTheShipThatOccupiesTheCoordinate()
        {
            AssertContains5Ships();

            //Arrange
            Mock<IShip>[] shipMocks = ReplaceShipsWithMocks();
            Mock<IShip> matchingShipMock = shipMocks.NextRandomElement();
            matchingShipMock.Setup(s => s.CanBeFoundAtCoordinate(It.IsAny<GridCoordinate>())).Returns(true);
            IShip matchingShip = matchingShipMock.Object;

            GridCoordinate coordinate = new GridCoordinateBuilder().Build();

            //Act
            IShip result = _fleet.FindShipAtCoordinate(coordinate);

            //Act + Assert
            matchingShipMock.Verify(s => s.CanBeFoundAtCoordinate(coordinate), Times.AtLeastOnce,
                "Use the CanBeFoundAtCoordinate of the Ship class to determine if a ship occupies the coordinate");
            Assert.That(result, Is.SameAs(matchingShip));
        }

        [MonitoredTest("FindShipAtCoordinate - Should return null when no ship occupies the coordinate")]
        public void FindShipAtCoordinate_ShouldReturnNullWhenNoShipOccupiesTheCoordinate()
        {
            AssertContains5Ships();

            //Arrange
            Mock<IShip>[] shipMocks = ReplaceShipsWithMocks();

            GridCoordinate coordinate = new GridCoordinateBuilder().Build();

            //Act
            IShip result = _fleet.FindShipAtCoordinate(coordinate);

            //Act + Assert
            Assert.That(result, Is.Null);
            foreach (Mock<IShip> shipMock in shipMocks)
            {
                IShip ship = shipMock.Object;
                shipMock.Verify(s => s.CanBeFoundAtCoordinate(coordinate), Times.Once,
                    $"The CanBeFoundAtCoordinate of the Ship is not called for the '{ship.Kind.Name}'. " +
                    $"Each ship must be checked.");
            }
        }

        [MonitoredTest("GetAllShips - Should return all ships in the dictionary")]
        public void GetAllShips_ShouldShouldReturnAllShipsInTheDictionary()
        {
            AssertContains5Ships();

            //Act
            IList<IShip> allShips = _fleet.GetAllShips();

            //Act + Assert
            Assert.That(allShips, Has.Count.EqualTo(5));
            foreach (IShip ship in _internalDictionary.Values)
            {
                Assert.That(allShips, Contains.Item(ship), $"The ship '{ship.Kind.Name}' is missing.");
            }
        }

        [MonitoredTest("GetSunkenShips - Should return the sunken ships in the dictionary")]
        public void GetSunkenShips_ShouldReturnTheSunkenShipsInTheDictionary()
        {
            AssertContains5Ships();

            //Arrange
            Mock<IShip>[] shipMocks = ReplaceShipsWithMocks();
            int numberOfSunkenShips = RandomGenerator.Next(1, 5);
            foreach (Mock<IShip> shipMock in shipMocks.Take(numberOfSunkenShips))
            {
                shipMock.SetupGet(s => s.HasSunk).Returns(true);
            }

            //Act
            IList<IShip> sunkenShips = _fleet.GetSunkenShips();

            //Act + Assert
            Assert.That(sunkenShips, Has.Count.EqualTo(numberOfSunkenShips));
            foreach (IShip ship in sunkenShips)
            {
                Assert.That(_internalDictionary, Contains.Value(ship),
                    $"The ship '{ship.Kind.Name}' in the returned list cannot be found in the internal dictionary of ships.");
            }
        }

        [MonitoredTest("TryMoveShipTo - Should position the ship on the grid when the coordinates are valid")]
        public void TryMoveShipTo_ShouldPositionTheShipOnTheGridWhenTheCoordinatesAreValid()
        {
            AssertContains5Ships();

            //Arrange
            Mock<IShip>[] shipMocks = ReplaceShipsWithMocks();
            foreach (Mock<IShip> shipMock in shipMocks)
            {
                shipMock.SetupGet(s => s.Squares).Returns(() => null);
            }

            Mock<IShip> shipToMoveMock = shipMocks.NextRandomElement();
            IShip shipToMove = shipToMoveMock.Object;

            GridCoordinate[] targetCoordinates = CreateAlignedAndLinkedGridCoordinates(shipToMove.Kind.Size, out _);

            //Act
            Result result = _fleet.TryMoveShipTo(shipToMove.Kind, targetCoordinates, _gridMock.Object);

            //Assert
            Assert.That(result.IsSuccess, Is.True, "Valid segment coordinates should result in a Success.");
            _gridMock.Verify(g => g.GetSquareAt(It.IsIn(targetCoordinates)), Times.Exactly(targetCoordinates.Length),
                "The GetSquareAt method of the grid should have been called for each segment coordinate.");

            shipToMoveMock.Verify(
                s => s.PositionOnGrid(It.Is<IGridSquare[]>(gridSquares =>
                    gridSquares.All(gs => targetCoordinates.Contains(gs.Coordinate)))), Times.Once,
                "The PositionOnGrid method should have been called for the ship that is being moved. " +
                "The GridSquares that are passed should match the segment coordinates.");
        }

        [MonitoredTest("TryMoveShipTo - Should fail when segment coordinates do not match the length of the ship")]
        public void TryMoveShipTo_ShouldFailWhenSegmentCoordinatesDoNotMatchLengthOfTheShip()
        {
            AssertContains5Ships();

            //Arrange
            IShip shipToMove = _internalDictionary.Values.NextRandomElement();

            int invalidSegmentSize = Math.Max(2, (shipToMove.Kind.Size + 1) % 6);
            GridCoordinate[] targetCoordinates = new GridCoordinate[invalidSegmentSize];
            for (int i = 0; i < invalidSegmentSize; i++)
            {
                targetCoordinates[i] = new GridCoordinateBuilder().Build();
            }

            //Act
            Result result = _fleet.TryMoveShipTo(shipToMove.Kind, targetCoordinates, _gridMock.Object);

            //Assert
            Assert.That(result.IsFailure, Is.True);
        }

        [MonitoredTest("TryMoveShipTo - Should fail when one of the segment coordinates is outside the grid")]
        public void TryMoveShipTo_ShouldFailWhenASegmentCoordinateIsOutsideTheGrid()
        {
            AssertContains5Ships();

            //Arrange
            IShip shipToMove = _internalDictionary.Values.NextRandomElement();

            GridCoordinate[] targetCoordinates = new GridCoordinate[shipToMove.Kind.Size];
            for (int i = 0; i < shipToMove.Kind.Size; i++)
            {
                targetCoordinates[i] = new GridCoordinateBuilder().Build();
            }

            int invalidCoordinateIndex = RandomGenerator.Next(0, shipToMove.Kind.Size);
            int gridSize = _gridMock.Object.Size;
            int invalidRow = RandomGenerator.Next(gridSize, gridSize + 1);
            int invalidColumn = RandomGenerator.Next(-2, 0);
            targetCoordinates[invalidCoordinateIndex] = new GridCoordinate(invalidRow, invalidColumn);

            //Act
            Result result = _fleet.TryMoveShipTo(shipToMove.Kind, targetCoordinates, _gridMock.Object);

            //Assert
            Assert.That(result.IsFailure, Is.True);
        }

        [MonitoredTest("TryMoveShipTo - Should fail when the segment coordinates are not aligned")]
        public void TryMoveShipTo_ShouldFailWhenTheSegmentCoordinatesAreNotAligned()
        {
            AssertContains5Ships();

            //Arrange
            IShip shipToMove = _internalDictionary.Values.NextRandomElement();

            GridCoordinate[] targetCoordinates = CreateAlignedAndLinkedGridCoordinates(shipToMove.Kind.Size, out _);
            
            int invalidCoordinateIndex = RandomGenerator.Next(0, shipToMove.Kind.Size);
            int gridSize = _gridMock.Object.Size;

            int invalidRow = (targetCoordinates[invalidCoordinateIndex].Row + 2) % gridSize ;
            int invalidColumn = (targetCoordinates[invalidCoordinateIndex].Column + 2) % gridSize;
            targetCoordinates[invalidCoordinateIndex] = new GridCoordinate(invalidRow, invalidColumn);

            //Act
            Result result = _fleet.TryMoveShipTo(shipToMove.Kind, targetCoordinates, _gridMock.Object);

            //Assert
            Assert.That(result.IsFailure, Is.True,
                $"Moving '{shipToMove.Kind.Name}' ship to {targetCoordinates.Print()} should fail but doesn't. " +
                $"Grid size = {gridSize}.");
        }

        [MonitoredTest("TryMoveShipTo - Should fail when the segment coordinates are not linked")]
        public void TryMoveShipTo_ShouldFailWhenTheSegmentCoordinatesAreNotLinked()
        {
            AssertContains5Ships();

            //Arrange
            IShip shipToMove = _internalDictionary.Values.NextRandomElement();

            GridCoordinate[] targetCoordinates = CreateAlignedAndLinkedGridCoordinates(shipToMove.Kind.Size, out bool horizontal)
                                                    .OrderByDescending(c => horizontal ? c.Column : c.Row)
                                                    .ToArray();
            int gridSize = _gridMock.Object.Size;

            //break the link
            int row = horizontal ? targetCoordinates[0].Row : (targetCoordinates.Max(c => c.Row) + 1) % gridSize;
            int column = horizontal ? (targetCoordinates.Max(c => c.Column) + 1) % gridSize : targetCoordinates[0].Column;
            targetCoordinates[0] = new GridCoordinate(row, column);

            //Act
            Result result = _fleet.TryMoveShipTo(shipToMove.Kind, targetCoordinates, _gridMock.Object);

            //Assert
            Assert.That(result.IsFailure, Is.True);
        }

        [MonitoredTest("TryMoveShipTo - Should fail when the segment coordinates collide with another ship")]
        public void TryMoveShipTo_ShouldFailWhenTheSegmentCoordinatesCollideWithAnotherShip()
        {
            AssertContains5Ships();

            //Arrange
            Mock<IShip>[] shipMocks = ReplaceShipsWithMocks();

            Mock<IShip> shipToMoveMock = shipMocks.NextRandomElement(out int shipToMoveIndex);
            IShip shipToMove = shipToMoveMock.Object;
            shipToMoveMock.SetupGet(s => s.Squares).Returns(() => null);

            Mock<IShip> collidingShipMock = shipMocks[(shipToMoveIndex + RandomGenerator.Next(1,4)) % shipMocks.Length];
            collidingShipMock.Setup(s => s.CanBeFoundAtCoordinate(It.IsAny<GridCoordinate>())).Returns(true);

            GridCoordinate[] targetCoordinates = CreateAlignedAndLinkedGridCoordinates(shipToMove.Kind.Size, out _);

            //Act
            Result result = _fleet.TryMoveShipTo(shipToMove.Kind, targetCoordinates, _gridMock.Object);

            //Assert
            Assert.That(result.IsFailure, Is.True);
            collidingShipMock.Verify(s => s.CanBeFoundAtCoordinate(It.IsIn(targetCoordinates)), Times.AtLeastOnce,
                "Use the CanBeFoundAtCoordinate method of Ship to check if a ship collides with one of the segment coordinates.");
            shipToMoveMock.Verify(s => s.CanBeFoundAtCoordinate(It.IsAny<GridCoordinate>()), Times.Never,
                "Do not use the CanBeFoundAtCoordinate method for the ship that is being moved. A ship cannot collide with itself.");

        }

        [MonitoredTest("RandomlyPositionOnGrid - Should create random segment coordinates for each ship")]
        public void RandomlyPositionOnGrid_ShouldCreateRandomSegmentCoordinatesForEachShip()
        {
            AssertContains5Ships();

            //Act
             _fleet.RandomlyPositionOnGrid(_gridMock.Object);

            //Assert

            int gridSize = _gridMock.Object.Size;
            foreach (IShip ship in _fleet.GetAllShips())
            {
                Assert.That(ship.Squares, Is.Not.Null,
                    $"The '{ship.Kind.Name}' is not positioned on the grid. Squares is null.");
                var coordinates = ship.Squares.Select(s => s.Coordinate).ToArray();
                Assert.That(coordinates.AreAligned, Is.True,
                    $"The coordinates {coordinates.Print()} of '{ship.Kind.Name}' are not aligned.");
                Assert.That(coordinates.AreLinked, Is.True,
                    $"The coordinates {coordinates.Print()} of '{ship.Kind.Name}' are not linked.");
                Assert.That(coordinates.HasAnyOutOfBounds(gridSize), Is.False,
                    $"The coordinates {coordinates.Print()} of '{ship.Kind.Name}' are not all inside the grid boundaries.");
            }
        }

        [MonitoredTest("RandomlyPositionOnGrid - Should use existing methods")]
        public void RandomlyPositionOnGrid_ShouldUseExistingMethods()
        {
            var code = Solution.Current.GetFileContent(@"BattleShip.Domain\FleetDomain\Fleet.cs");
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var method = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(md => md.Identifier.ValueText.Contains("RandomlyPositionOnGrid"));

            Assert.That(method, Is.Not.Null, "Could not find the 'RandomlyPositionOnGrid' method.");

            var numberOfForLoops = method.Body
                .DescendantNodes().Count(node => node is ForEachStatementSyntax || node is ForStatementSyntax);

            Assert.That(numberOfForLoops, Is.EqualTo(1),
                "There must be exactly one for loop that loops over each ship in the fleet");

            var body = CodeCleaner.StripComments(method.Body.ToString());

            Assert.That(body, Contains.Substring(".Kind.GenerateRandomSegmentCoordinates(grid.Size").IgnoreCase,
                "You must use the GenerateRandomSegmentCoordinates of the ShipKind class to generate random segment coordinates for each ship");

            Assert.That(body, Contains.Substring("TryMoveShipTo(").IgnoreCase,
                "You must use the TryMoveShipTo method to position each ship on the grid.");

            var numberOfWhileLoops = method.Body
                .DescendantNodes().Count(node => node is WhileStatementSyntax);

            Assert.That(numberOfWhileLoops, Is.EqualTo(1),
                "You must use exactly one while loop. For each ship you must try to move it onto the grid until it succeeds.");
        }

        private GridCoordinate[] CreateAlignedAndLinkedGridCoordinates(int shipSize, out bool horizontal)
        {
            horizontal = RandomGenerator.NextBool();
            int row = RandomGenerator.Next(0, 6);
            int column = RandomGenerator.Next(0, 6);
            GridCoordinate[] targetCoordinates = new GridCoordinate[shipSize];
            for (int i = 0; i < shipSize; i++)
            {
                targetCoordinates[i] = new GridCoordinate(row, column);
                if (horizontal)
                {
                    column += 1;
                }
                else
                {
                    row += 1;
                }

            }
            return targetCoordinates;
        }

        private Mock<IShip>[] ReplaceShipsWithMocks()
        {
            var shipMocks = new Mock<IShip>[_internalDictionary.Count];
            for (var index = 0; index < ShipKind.All.Length; index++)
            {
                ShipKind shipKind = ShipKind.All[index];
                shipMocks[index] = new ShipBuilder(shipKind).WithSquares().BuildMock();
                _internalDictionary[shipKind] = shipMocks[index].Object;
            }

            return shipMocks;
        }

        private void AssertContains5Ships()
        {
            Assert.That(_internalDictionary, Is.Not.Null,
                "The Fleet class should use a private field of the type Dictionary<TKey, TValue> " +
                "where TKey is ShipKind and TValue is IShip.");
            Assert.That(_internalDictionary.Count, Is.EqualTo(5), "The dictionary should contain exactly 5 ships.");
            foreach (ShipKind shipKind in ShipKind.All)
            {
                Assert.That(_internalDictionary.ContainsKey(shipKind), Is.True,
                    $"The dictionary should contain a key '{shipKind.Name}'.");
                Assert.That(_internalDictionary[shipKind], Is.Not.Null,
                    $"The value in the dictionary for '{shipKind.Name}' must be an instance of a class that implements IShip.");
                Assert.That(_internalDictionary[shipKind].Kind, Is.EqualTo(shipKind),
                    $"The value in the dictionary for '{shipKind.Name}' must be a ship with Kind set to '{shipKind.Name}'.");
            }
        }
    }
}
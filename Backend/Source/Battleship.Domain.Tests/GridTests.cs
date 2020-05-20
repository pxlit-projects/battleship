using System;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "Grid", @"Battleship.Domain\GridDomain\Grid.cs")]
    public class GridTests : TestBase
    {
        [MonitoredTest("Constructor - Should initialize the grid squares")]
        public void Constructor_ShouldInitializeTheGridSquares()
        {
            //Arrange
            int size = RandomGenerator.Next(10, 16);

            //Act
            Grid grid = new Grid(size);

            //Assert
            Assert.That(grid.Size, Is.EqualTo(size), "Size property is not initialized correctly.");
            Assert.That(grid.Squares, Is.Not.Null, "The matrix of GridSquares is not initialized.");
            Assert.That(grid.Squares.GetLength(0), Is.EqualTo(size),
                "The number of rows of the GridSquare matrix should be equal to the size of the grid");
            Assert.That(grid.Squares.GetLength(1), Is.EqualTo(size),
                "The number of columns of the GridSquare matrix should be equal to the size of the grid");
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    IGridSquare square = grid.Squares[i, j];
                    GridCoordinate expectedCoordinate = new GridCoordinate(i, j);
                    Assert.That(square, Is.Not.Null, $"The GridSquare at {expectedCoordinate} is null");
                    Assert.That(square.Coordinate, Is.EqualTo(expectedCoordinate),
                        $"The GridSquare at {expectedCoordinate} does not have a matching coordinate: {square.Coordinate}");
                }
            }
        }

        [MonitoredTest("GetSquareAt - Should return the correct square")]
        public void GetSquareAt_ShouldReturnCorrectSquare()
        {
            //Arrange
            int gridSize = RandomGenerator.Next(10, 16);
            Grid grid = new Grid(gridSize);

            AssertSquaresAreInitialized(grid, gridSize);

            GridCoordinate coordinate = new GridCoordinateBuilder(gridSize).Build();

            //Act
            IGridSquare square = grid.GetSquareAt(coordinate);

            //Assert
            Assert.That(square, Is.Not.Null);
            Assert.That(square.Coordinate, Is.EqualTo(coordinate));
            Assert.That(grid.Squares, Has.One.SameAs(square),
                "The IGridSquare returned is not one of the squares of the grid. Did you perhaps returned a new instance of GridSquare?");
        }

        [MonitoredTest("Shoot - Should hit the matching square with a bomb and return it")]
        public void Shoot_ShouldHitTheMatchingSquareWithABombAndReturnIt()
        {
            //Arrange
            int gridSize = RandomGenerator.Next(10, 16);
            Grid grid = new Grid(gridSize);

            AssertSquaresAreInitialized(grid, gridSize);

            GridCoordinate coordinate = new GridCoordinateBuilder(gridSize).Build();

            //Act
            IGridSquare square = grid.Shoot(coordinate);

            //Assert
            Assert.That(square, Is.Not.Null);
            Assert.That(square.Coordinate, Is.EqualTo(coordinate), "The wrong square is shot.");
            Assert.That(square.Status, Is.Not.EqualTo(GridSquareStatus.Untouched),
                "After hitting a square, its status should not be Untouched. " +
                "Use the HitByBomb method of the square to achieve this.");
            Assert.That(grid.Squares, Has.One.SameAs(square),
                "The IGridSquare returned is not one of the squares of the grid. Did you perhaps returned a new instance of GridSquare?");
        }

        [MonitoredTest("Shoot - Should throw ApplicationException when the shot is not within the grid")]
        public void Shoot_ShouldThrowApplicationExceptionWhenTheShotIsNotWithinTheGrid()
        {
            //Arrange
            int gridSize = RandomGenerator.Next(10, 16);
            Grid grid = new Grid(gridSize);
            GridCoordinate outOfBoundCoordinate = new GridCoordinate(gridSize, gridSize);

            //Act + Assert
            Assert.That(() => grid.Shoot(outOfBoundCoordinate), Throws.InstanceOf<ApplicationException>());
        }

        private void AssertSquaresAreInitialized(Grid grid, int gridSize)
        {
            if (grid.Squares == null || grid.Squares.Length != gridSize * gridSize)
            {
                Assert.Fail(
                    $"Grid squares are not properly initialized. Make sure the test '{nameof(Constructor_ShouldInitializeTheGridSquares)}' is green first.");
            }
        }
    }
}
using Battleship.Domain.GridDomain;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "GridCoordinate", @"Battleship.Domain\GridDomain\GridCoordinate.cs")]
    public class GridCoordinateTests : TestBase
    {
        [MonitoredTest("CreateRandom - Should create a coordinate within the bounds determined by the grid size")]
        public void CreateRandom_ShouldCreateACoordinateWithinTheBoundsOfGridSize()
        {
            for (int gridSize = 1; gridSize <= 15; gridSize++)
            {
                AssertRandomCoordinateCreation(gridSize);
            }
        }

        [MonitoredTest("CreateRandom - Should be random")]
        public void CreateRandom_ShouldBeRandom()
        {
            int gridSize = 1000;
            bool isRandom = false;
            GridCoordinate previousCoordinate = GridCoordinate.CreateRandom(gridSize);
            int numberOfTries = 0;

            while (numberOfTries <= 15 && !isRandom)
            {
                GridCoordinate newCoordinate = GridCoordinate.CreateRandom(gridSize);
                if (newCoordinate != previousCoordinate)
                {
                    isRandom = true;
                }

                previousCoordinate = newCoordinate;
                numberOfTries++;
            }
            Assert.That(isRandom, Is.True);
        }

        [MonitoredTest("IsOutOfBounds - Should return true when coordinate is not within grid")]
        [TestCase(-1, 0, 10, true)]
        [TestCase(0, -1, 10, true)]
        [TestCase(11, 0, 10, true)]
        [TestCase(0, 11, 10, true)]
        [TestCase(5, 5, 10, false)]
        [TestCase(0, 0, 1, false)]
        [TestCase(1, 1, 1, true)]
        public void IsOutOfBounds_ShouldReturnTrueWhenCoordinateIsNotWithinGrid(int row, int column, int gridSize, bool expected)
        {
            GridCoordinate coordinate = new GridCoordinate(row, column);
            Assert.That(coordinate.IsOutOfBounds(gridSize), Is.EqualTo(expected),
                $"Coordinate ({row},{column}) should result in '{expected}'");
        }

        [MonitoredTest("GetNeighbor - Should return the adjacent coordinate in the given direction")]
        public void GetNeighbor_ShouldReturnAdjacentCoordinateInTheGivenDirection()
        {
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();
            foreach (Direction direction in Direction.AllDirections)
            {
                AssertGettingNeighbor(coordinate, direction);
            }
        }

        [MonitoredTest("GetOtherEnd - Should return the coordinate some distance in a given direction")]
        public void GetOtherEnd_ShouldReturnTheCoordinateSomeDistanceInAGivenDirection()
        {
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();
            foreach (Direction direction in Direction.AllDirections)
            {
                int distance = RandomGenerator.Next(2, 6);
                AssertGettingOtherEnd(coordinate, direction, distance);
            }
        }

        private void AssertRandomCoordinateCreation(int gridSize)
        {
            //Act
            GridCoordinate coordinate = GridCoordinate.CreateRandom(gridSize);

            //Assert
            Assert.That(coordinate.Row >= 0 && coordinate.Row < gridSize, Is.True,
                $"For a grid of size {gridSize}, the row must be greater than or equal to 0 and smaller than {gridSize}");
            Assert.That(coordinate.Column >= 0 && coordinate.Column < gridSize, Is.True,
                $"For a grid of size {gridSize}, the column must be greater than or equal to 0 and smaller than {gridSize}");
        }

        private void AssertGettingNeighbor(GridCoordinate coordinate, Direction direction)
        {
            //Act
            GridCoordinate neighbor = coordinate.GetNeighbor(direction);

            //Assert
            int expectedRow = coordinate.Row + direction.YStep;
            int expectedColumn = coordinate.Column + direction.XStep;
            Assert.That(neighbor.Row, Is.EqualTo(expectedRow),
                $"The row of the neighbor of {coordinate} in direction '{direction}' should be {expectedRow}");
            Assert.That(neighbor.Column, Is.EqualTo(expectedColumn),
                $"The column of the neighbor of {coordinate} in direction '{direction}' should be {expectedColumn}");
        }

        private void AssertGettingOtherEnd(GridCoordinate coordinate, Direction direction, in int distance)
        {
            //Act
            GridCoordinate otherEnd = coordinate.GetOtherEnd(direction, distance);

            //Assert
            int expectedRow = coordinate.Row + direction.YStep * distance;
            int expectedColumn = coordinate.Column + direction.XStep * distance;
            Assert.That(otherEnd.Row, Is.EqualTo(expectedRow),
                $"The row of the coordinate {distance} steps away from {coordinate} in direction '{direction}' should be {expectedRow}");
            Assert.That(otherEnd.Column, Is.EqualTo(expectedColumn),
                $"The column of the coordinate {distance} steps away from {coordinate} in direction '{direction}' should be {expectedColumn}");
        }
    }
}
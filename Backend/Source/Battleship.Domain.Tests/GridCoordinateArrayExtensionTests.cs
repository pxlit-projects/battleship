using Battleship.Domain.GridDomain;
using Battleship.TestTools;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    public class GridCoordinateArrayExtensionTests : TestBase
    {
        [MonitoredTest("HasAnyOutOfBounds - Should return false if all coordinates are on the grid")]
        public void HasAnyOutOfBounds_ShouldReturnFalseIfAllCoordinatesAreOnTheGrid()
        {
            //Arrange
            GridCoordinate[] coordinates =
            {
                new GridCoordinate(0, 0), new GridCoordinate(2, 2), new GridCoordinate(4, 4), new GridCoordinate(9, 9)
            };
            int gridSize = 10;


            //Act + Assert
            Assert.That(coordinates.HasAnyOutOfBounds(gridSize), Is.False);
        }

        [MonitoredTest("HasAnyOutOfBounds - Should return true if one of the coordinates is not on the grid")]
        public void HasAnyOutOfBounds_ShouldReturnTrueIfOneOfTheCoordinatesIsNotOnTheGrid()
        {
            //Arrange
            int gridSize = 10;
            GridCoordinate[] coordinates =
            {
                new GridCoordinate(0, 0),
                new GridCoordinate(2, 2),
                new GridCoordinate(4, 4),
                new GridCoordinate(9, 9),
                new GridCoordinate(RandomGenerator.Next(10, 21), 1),
                new GridCoordinate(1, RandomGenerator.Next(-10, 0))
            };

            //Act + Assert
            Assert.That(coordinates.HasAnyOutOfBounds(gridSize), Is.True);
        }

        [MonitoredTest("AreHorizontallyAligned - Should return true when coordinates are all in the same row")]
        public void AreHorizontallyAligned_ShouldReturnTrueWhenCoordinatesAreAllInTheSameRow()
        {
            //Arrange
            int row = RandomGenerator.Next(11);
            GridCoordinate[] coordinates =
            {
                new GridCoordinate(row, 0), new GridCoordinate(row, 2), new GridCoordinate(row, 4), new GridCoordinate(row, 9)
            };

            //Act
            Assert.That(coordinates.AreHorizontallyAligned(), Is.True);
        }

        [MonitoredTest("AreHorizontallyAligned - Should return false when one of the coordinates is in another row")]
        public void AreHorizontallyAligned_ShouldReturnFalseWhenOneOfTheCoordinatesIsInAnotherRow()
        {
            //Arrange
            int row = RandomGenerator.Next(11);
            GridCoordinate[] coordinates =
            {
                new GridCoordinate(row, 0), new GridCoordinate(row, 2), new GridCoordinate(row + 1, 4), new GridCoordinate(row - 1, 9)
            };

            //Act
            Assert.That(coordinates.AreHorizontallyAligned(), Is.False);
        }

        [MonitoredTest("AreVerticallyAligned - Should return true when coordinates are all in the same column")]
        public void AreVerticallyAligned_ShouldReturnTrueWhenCoordinatesAreAllInTheSameColumn()
        {
            //Arrange
            int column = RandomGenerator.Next(11);
            GridCoordinate[] coordinates =
            {
                new GridCoordinate(0, column), new GridCoordinate(2, column), new GridCoordinate(4, column), new GridCoordinate(9, column)
            };

            //Act
            Assert.That(coordinates.AreVerticallyAligned, Is.True);
        }

        [MonitoredTest("AreVerticallyAligned - Should return false when one of the coordinates is in another column")]
        public void AreVerticallyAligned_ShouldReturnFalseWhenOneOfTheCoordinatesIsInAnotherColumn()
        {
            //Arrange
            int column = RandomGenerator.Next(11);
            GridCoordinate[] coordinates =
            {
                new GridCoordinate(0, column), new GridCoordinate(2, column), new GridCoordinate(4, column + 1), new GridCoordinate(9, column - 1)
            };

            //Act
            Assert.That(coordinates.AreVerticallyAligned(), Is.False);
        }

        [MonitoredTest("AreAligned - Should return true when coordinates are horizontally or vertically aligned")]
        public void AreAligned_ShouldReturnTrueWhenCoordinatesAreAllHorizontallyOrVerticallyAligned()
        {
            //Arrange
            int row = RandomGenerator.Next(11);
            int column = RandomGenerator.Next(11);
            GridCoordinate[] horizontalAlignedCoordinates =
            {
                new GridCoordinate(row, 0), new GridCoordinate(row, 1), new GridCoordinate(row, 2), new GridCoordinate(row, 3)
            };
            GridCoordinate[] verticalAlignedCoordinates =
            {
                new GridCoordinate(3, column), new GridCoordinate(4, column), new GridCoordinate(6, column), new GridCoordinate(7, column)
            };

            //Act
            Assert.That(horizontalAlignedCoordinates.AreAligned(), Is.True, "Should return true when coordinates are horizontally aligned");
            Assert.That(verticalAlignedCoordinates.AreAligned(), Is.True, "Should return true when coordinates are vertically aligned");
        }

        [MonitoredTest("AreLinked - Should return true when all coordinates are touching each other")]
        public void AreLinked_ShouldReturnTrueWhenAllCoordinatesAreTouchingEachOther()
        {
            //Arrange
            GridCoordinate[] touching1 = {new GridCoordinate(0, 0), new GridCoordinate(1, 1), new GridCoordinate(2, 2)};
            GridCoordinate[] touching2 = { new GridCoordinate(2, 5), new GridCoordinate(3, 5), new GridCoordinate(3, 6) };

            //Act
            Assert.That(touching1.AreLinked(), Is.True, $"Should be true for {touching1.Print()}");
            Assert.That(touching2.AreLinked(), Is.True, $"Should be true for {touching2.Print()}");
        }

        [MonitoredTest("AreLinked - Should return false when some coordinates are not touching each other")]
        public void AreLinked_ShouldReturnFalseWhenSomeCoordinatesAreNotTouching()
        {
            //Arrange
            GridCoordinate[] notTouching1 = { new GridCoordinate(0, 0), new GridCoordinate(1, 1), new GridCoordinate(1, 3) };
            GridCoordinate[] notTouching2 = { new GridCoordinate(2, 4), new GridCoordinate(2, 6), new GridCoordinate(2, 7) };

            //Act
            Assert.That(notTouching1.AreLinked(), Is.False, $"Should be false for {notTouching1.Print()}");
            Assert.That(notTouching2.AreLinked(), Is.False, $"Should be false for {notTouching2.Print()}");
        }

        [MonitoredTest("AreLinked - Should return false when some coordinates are on top of each other")]
        public void AreLinked_ShouldReturnFalseWhenSomeCoordinatesAreOnTopOfEachOther()
        {
            //Arrange
            GridCoordinate[] coordinates = { new GridCoordinate(0, 0), new GridCoordinate(0, 0), new GridCoordinate(1, 0) };

            //Act
            Assert.That(coordinates.AreLinked(), Is.False, $"Should be false for {coordinates.Print()}");
        }

        
    }
}
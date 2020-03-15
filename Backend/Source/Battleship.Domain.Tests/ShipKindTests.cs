using System.Linq;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GridDomain;
using Battleship.TestTools;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    public class ShipKindTests : TestBase
    {
        [MonitoredTest("GenerateRandomSegmentCoordinates - Should generate valid random ship segment coordinates")]
        public void GenerateRandomSegmentCoordinates_ShouldGenerateValidRandomShipSegmentCoordinates()
        {
            foreach (var kind in ShipKind.All)
            {
                int gridSize = 15;

                AssertRandomCoordinateGeneration(kind, gridSize, out var previousCoordinates);

                int numberOfTries = 10;
                int numberOfEqualities = 0;

                for (int i = 0; i < numberOfTries; i++)
                {
                    AssertRandomCoordinateGeneration(kind, gridSize, out var currentCoordinates);

                    if (AreCoordinatesEqual(previousCoordinates, currentCoordinates))
                    {
                        numberOfEqualities++;
                    }

                    previousCoordinates = currentCoordinates;
                }

                Assert.That(numberOfEqualities, Is.LessThanOrEqualTo(3), "The method does not seem to be random enough.");
            }
        }

        private bool AreCoordinatesEqual(GridCoordinate[] a, GridCoordinate[] b)
        {
            if (a.Length != b.Length) return false;
            return !a.Where((coordinate, index) => coordinate != b[index]).Any();
        }

        private void AssertRandomCoordinateGeneration(ShipKind kind, int gridSize, out GridCoordinate[] generatedCoordinates)
        {
            //Act
            generatedCoordinates = kind.GenerateRandomSegmentCoordinates(gridSize);

            //Assert
            Assert.That(generatedCoordinates.Length, Is.EqualTo(kind.Size),
                $"The number of coordinates must be equal to the size of the shipKind ({kind.Name} has size {kind.Size}).");
            Assert.That(generatedCoordinates.HasAnyOutOfBounds(gridSize), Is.False,
                $"{generatedCoordinates.Print()} is out of bounds for grid with size {gridSize}.");
            Assert.That(generatedCoordinates.AreLinked(), Is.True,
                $"{generatedCoordinates.Print()} has two or more coordinates that are not linked.");
            Assert.That(generatedCoordinates.AreAligned(), Is.True,
                $"{generatedCoordinates.Print()} has two or more coordinates that are not aligned.");
        }
    }
}
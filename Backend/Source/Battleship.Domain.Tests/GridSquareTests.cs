using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "GridSquare", @"Battleship.Domain\GridDomain\GridSquare.cs")]
    public class GridSquareTests : TestBase
    {
        private bool _onHitByBombEventTriggered;

        [SetUp]
        public void Setup()
        {
            _onHitByBombEventTriggered = false;
        }

        [MonitoredTest("Constructor - Should set defaults")]
        public void Constructor_ShouldSetDefaults()
        {
            //Arrange
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();

            //Act
            GridSquare square = new GridSquare(coordinate);

            //Assert
            Assert.That(square.Coordinate, Is.SameAs(coordinate));
            Assert.That(square.NumberOfBombs, Is.Zero);
            Assert.That(square.Status, Is.EqualTo(GridSquareStatus.Untouched));
        }

        [MonitoredTest("HitByBomb - Should register the hit as a Miss and invoke the OnHitByBomb event")]
        public void HitByBomb_ShouldRegisterTheHitAsAMissAndInvokeTheOnHitByBombEvent()
        {
            //Arrange
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();
            GridSquare square = new GridSquare(coordinate);
            square.OnHitByBomb += Square_OnHitByBomb;

            //Act
            square.HitByBomb();

            //Assert
            Assert.That(square.NumberOfBombs, Is.EqualTo(1), "The number of bombs after one hit should be 1.");
            Assert.That(square.Status, Is.EqualTo(GridSquareStatus.Miss),
                "When a square is hit by a bomb it gets the Status Miss " +
                "(This Status can change after invoking the OnHitByBomb event).");
            Assert.That(_onHitByBombEventTriggered, Is.True,
                "The OnHitByBomb event is not invoked. " +
                "You can invoke the event with this statement: 'OnHitByBomb?.Invoke(this);'.");
        }

        [MonitoredTest("EXTRA - HitByBomb - Should increase number of bombs on each hit")]
        public void EXTRA_HitByBomb_ShouldIncreaseNumberOfBombsOnEachHit()
        {
            //Arrange
            GridCoordinate coordinate = new GridCoordinateBuilder().Build();
            GridSquare square = new GridSquare(coordinate);

            //Act
            int numberOfHits = RandomGenerator.Next(4, 11);
            for (int i = 0; i < numberOfHits; i++)
            {
                square.HitByBomb();
            }
            
            //Assert
            Assert.That(square.NumberOfBombs, Is.EqualTo(numberOfHits),
                $"The number of bombs after one hit should be {numberOfHits} after calling HitByBomb {numberOfHits} times.");
        }

        private void Square_OnHitByBomb(IGridSquare sender)
        {
            _onHitByBombEventTriggered = true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.Domain.PlayerDomain;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Core;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Battleship", "RandomShooting", @"Battleship.Domain\PlayerDomain\RandomShootingStrategy.cs")]
    public class RandomShootingStrategyTests : TestBase
    {
        private RandomShootingStrategy _strategy;
        private GridBuilder _gridBuilder;
        private IGrid _grid;

        [SetUp]
        public void Setup()
        {
            var settings = new GameSettings();
            _gridBuilder = new GridBuilder(settings.GridSize);
            _grid = _gridBuilder.Build();

            _strategy = new RandomShootingStrategy(settings, _grid);
        }

        [MonitoredTest("DetermineTargetCoordinate - Should pick random coordinate within the grid")]
        public void DetermineTargetCoordinate_ShouldPickRandomCoordinatesWithinTheGrid()
        {
            int numberOfDeterminations = 20;
            int numberOfDifferentCoordinates = 0;
            GridCoordinate previousCoordinate = _strategy.DetermineTargetCoordinate();
            for (int i = 1; i < numberOfDeterminations; i++)
            {
                GridCoordinate currentCoordinate = _strategy.DetermineTargetCoordinate();
                Assert.That(currentCoordinate.IsOutOfBounds(_grid.Size), Is.False,
                    $"A coordinate {currentCoordinate} was determined. This coordinate is out of bounds.");
                if (currentCoordinate != previousCoordinate)
                {
                    numberOfDifferentCoordinates++;
                }

                previousCoordinate = currentCoordinate;
            }

            Assert.That(numberOfDifferentCoordinates, Is.GreaterThan(14),
                "The strategy is not random enough. " +
                $"Out of {numberOfDeterminations} determined coordinates, only {numberOfDifferentCoordinates} are different.");
        }

        [MonitoredTest("DetermineTargetCoordinate - Should only pick coordinates that are not hit by a bomb")]
        public void DetermineTargetCoordinate_ShouldOnlyPickCoordinatesThatAreNotHitByABomb()
        {
            //Arrange
            _gridBuilder.WithAllSquaresWithStatus(GridSquareStatus.Miss);
            //Set some squares to Hit
            for (int i = 0; i < 10; i++)
            {
                var coordinate = new GridCoordinateBuilder(_grid.Size).Build();
                _gridBuilder.WithSquareStatus(coordinate, GridSquareStatus.Hit);
            }

            //Set some squares to Untouched
            int numberOfUntouchedSquares = RandomGenerator.Next(2, _grid.Size + 1);
            var untouchedCoordinates = new List<GridCoordinate>();
            for (int i = 0; i < numberOfUntouchedSquares; i++)
            {
                var coordinate = new GridCoordinate(i, RandomGenerator.Next(0, _grid.Size));
                untouchedCoordinates.Add(coordinate);
                _gridBuilder.WithSquareStatus(coordinate, GridSquareStatus.Untouched);
            }

            //Act
            var determinedCoordinates = new List<GridCoordinate>();
            for (int i = 0; i < numberOfUntouchedSquares; i++)
            {
                var coordinate = _strategy.DetermineTargetCoordinate();
                determinedCoordinates.Add(coordinate);
                _gridBuilder.WithSquareStatus(coordinate, GridSquareStatus.Miss);
            }

            //Assert
            string invalidDeterminationMessage = $"When a grid has only {numberOfUntouchedSquares} untouched squares " +
                                                 $"and DetermineTargetCoordinate is called {numberOfUntouchedSquares} times, " +
                                                 "then all of the untouched squares should have been picked " +
                                                 "(this test marks a square as Missed after it is picked).";
            Assert.That(determinedCoordinates.Count, Is.EqualTo(numberOfUntouchedSquares), invalidDeterminationMessage);
            Assert.That(untouchedCoordinates.All(c => determinedCoordinates.Contains(c)), Is.True, invalidDeterminationMessage);
        }

        [MonitoredTest("DetermineTargetCoordinate - Should throw an ApplicationExceptionWhenAllSquaresAreAlreadyHit")]
        public void DetermineTargetCoordinate_ShouldThrowAnApplicationExceptionWhenAllSquaresAreAlreadyHit()
        {
            //Arrange
            _gridBuilder.WithAllSquaresWithStatus(GridSquareStatus.Miss);
            
            //Act + Assert
            Assert.That(() => _strategy.DetermineTargetCoordinate(), Throws.InstanceOf<ApplicationException>());
        }

        [MonitoredTest("DetermineTargetCoordinate - Should have good performance")]
        public void DetermineTargetCoordinate_ShouldHaveGoodPerformance()
        {
            //Arrange
            _gridBuilder.WithAllSquaresWithStatus(GridSquareStatus.Untouched);
            var stopWatch = new Stopwatch();
            int maximumNumberOfMilliseconds = 750;

            //Act
            stopWatch.Start();
            int numberOfDeterminations = (_grid.Size * _grid.Size) - _grid.Size;
            for (int i = 0; i < numberOfDeterminations; i++)
            {
                var coordinate = _strategy.DetermineTargetCoordinate();
                _gridBuilder.WithSquareStatus(coordinate, GridSquareStatus.Miss);
            }
            stopWatch.Stop();

            //Act
            Assert.That(stopWatch.Elapsed.TotalSeconds,
                Is.LessThan(TimeSpan.FromMilliseconds(maximumNumberOfMilliseconds).TotalSeconds),
                $"Doing {numberOfDeterminations} coordinate determinations should be completed in less than " +
                $"{TimeSpan.FromMilliseconds(maximumNumberOfMilliseconds).TotalSeconds} seconds.");
        }
    }
}
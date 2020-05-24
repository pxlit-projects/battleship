using System;
using System.Collections.Generic;
using System.Linq;
using Battleship.Domain.FleetDomain;
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
    [ProjectComponentTestFixture("1TINProject", "Battleship", "SmartShooting", @"Battleship.Domain\PlayerDomain\SmartShootingStrategy.cs")]
    public class SmartShootingStrategyTests : TestBase
    {
        private SmartShootingStrategy _strategy;
        private GridBuilder _gridBuilder;
        private IGrid _grid;

        [SetUp]
        public void Setup()
        {
            var settings = new GameSettings();
            _gridBuilder = new GridBuilder(settings.GridSize);
            _grid = _gridBuilder.Build();
            _strategy = new SmartShootingStrategy(settings, _grid);
        }

        [MonitoredTest("EXTRA - DetermineTargetCoordinate - Should only pick coordinates that are not hit by a bomb")]
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
            var untouchedCoordinates = new List<GridCoordinate>();
            for (int i = 0; i < RandomGenerator.Next(2, _grid.Size + 1); i++)
            {
                int column = RandomGenerator.Next(0, _grid.Size - 1);
                var coordinate1 = new GridCoordinate(i, column);
                untouchedCoordinates.Add(coordinate1);
                _gridBuilder.WithSquareStatus(coordinate1, GridSquareStatus.Untouched);

                var coordinate2 = new GridCoordinate(i, column + 1);
                untouchedCoordinates.Add(coordinate2);
                _gridBuilder.WithSquareStatus(coordinate2, GridSquareStatus.Untouched);
            }

            //Act
            var determinedCoordinates = new List<GridCoordinate>();
            int numberOfDeterminations = untouchedCoordinates.Count / 2;
            for (int i = 0; i < numberOfDeterminations; i++)
            {
                var coordinate = _strategy.DetermineTargetCoordinate();
                determinedCoordinates.Add(coordinate);
                _gridBuilder.WithSquareStatus(coordinate, GridSquareStatus.Miss);
            }

            //Assert
            string invalidDeterminationMessage = $"When a grid has only {untouchedCoordinates.Count} untouched squares " +
                                                 $"and DetermineTargetCoordinate is called {numberOfDeterminations} times, " +
                                                 $"then {numberOfDeterminations} of the untouched squares should have been picked " +
                                                 "(this test marks a square as Missed after it is picked).";
            Assert.That(determinedCoordinates.All(c => untouchedCoordinates.Contains(c)), Is.True, invalidDeterminationMessage);
        }

        [MonitoredTest("EXTRA - DetermineTargetCoordinate - Should pick random coordinate that can host an opponent ship")]
        [TestCaseSource(nameof(EXTRADetermineTargetCoordinateRandomSunkenShipTestCases))]
        public void DetermineTargetCoordinate_ShouldPickRandomCoordinatesThatCanHostAnOpponentShip(
            string gridConfiguration, int smallestOpponentShipSize, string allowedCoordinates, Dictionary<ShipKind, GridCoordinate[]> sunkenShips)
        {
            for (int numberOfChecks = 0; numberOfChecks < 10; numberOfChecks++)
            {
                //Arrange
                ParseGridConfiguration(gridConfiguration);

                foreach (var ship in sunkenShips)
                {
                    RegisterSunkenShip(ship.Key, ship.Value);
                }

                IList<GridCoordinate> expectedCoordinates = ParseExpectedCoordinates(allowedCoordinates);

                //Act
                GridCoordinate result = _strategy.DetermineTargetCoordinate();

                Assert.That(expectedCoordinates.Contains(result), Is.True,
                    $"Voor een grid met configuratie\n{gridConfiguration}\n " +
                    $"en waarbij het kleinste ongezonken schip van de tegenstander {smallestOpponentShipSize} segmenten heeft, \n" +
                    $"moet het gekozen coordinaat altijd één van volgende zijn: {allowedCoordinates}. " +
                    $"De strategie kiest echter soms {result}.\n " +
                    "(0=onaangeraakt, 1=gemist schot, 2=raak schot)");

                Setup();
            }
        }

        [MonitoredTest("EXTRA - DetermineTargetCoordinate - Should shoot a neighbor of a hit square")]
        public void DetermineTargetCoordinate_ShouldShootANeighborOfAHitSquare()
        {
            for (int numberOfChecks = 0; numberOfChecks < 5; numberOfChecks++)
            {
                //Arrange
                GridCoordinate hitCoordinate = new GridCoordinateBuilder(_grid.Size).Build();
                _gridBuilder.WithSquareStatus(hitCoordinate, GridSquareStatus.Hit);
                var ship = new ShipBuilder(ShipKind.Carrier).Build();
                _strategy.RegisterShotResult(hitCoordinate, ShotResult.CreateHit(ship, true));

                IList<GridCoordinate> expectedCoordinates = new List<GridCoordinate>();

                foreach (Direction direction in Direction.BasicDirections)
                {
                    GridCoordinate neighbor = hitCoordinate.GetNeighbor(direction);
                    if (!neighbor.IsOutOfBounds(_grid.Size))
                    {
                        expectedCoordinates.Add(neighbor);
                    }
                }

                //Act
                GridCoordinate result = _strategy.DetermineTargetCoordinate();

                Assert.That(expectedCoordinates.Contains(result), Is.True,
                    $"Bij een leeg grid met een raak shot op {hitCoordinate} " +
                    $"moet er geschoten worden op één van de volgende coordinaten: {expectedCoordinates.ToArray().Print()}. " +
                    $"De strategie kiest soms echter {result}");

                Setup();
            }
        }

        [MonitoredTest("EXTRA - DetermineTargetCoordinate - Should shoot in the right direction if two squares are hit")]
        public void DetermineTargetCoordinate_ShouldShootInTheRightDirectionIfTwoSquaresAreHit()
        {
            for (int numberOfChecks = 0; numberOfChecks < 10; numberOfChecks++)
            {
                //Arrange
                GridCoordinate hit1 = new GridCoordinate(RandomGenerator.Next(2, _grid.Size - 1), RandomGenerator.Next(2, _grid.Size - 1));
                int horizontalStep = RandomGenerator.Next(0, 2);
                int verticalStep = horizontalStep == 0 ? 1 : 0;
                GridCoordinate hit2 = new GridCoordinate(hit1.Row + verticalStep, hit1.Column + horizontalStep);
                var ship = new ShipBuilder(ShipKind.Carrier).Build();

                _gridBuilder.WithSquareStatus(hit1, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit1, ShotResult.CreateHit(ship, true));

                _gridBuilder.WithSquareStatus(hit2, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit2, ShotResult.CreateHit(ship, true));

                IList<GridCoordinate> expectedCoordinates = new List<GridCoordinate>();

                Direction hit2To1Direction = Direction.FromCoordinates(hit2, hit1);
                expectedCoordinates.Add(hit1.GetNeighbor(hit2To1Direction));

                Direction hit1To2Direction = hit2To1Direction.Opposite;
                expectedCoordinates.Add(hit2.GetNeighbor(hit1To2Direction));

                //Act
                GridCoordinate result = _strategy.DetermineTargetCoordinate();

                Assert.That(expectedCoordinates.Contains(result), Is.True,
                    $"Bij een grid met 2 rake schoten shot op {hit1} en {hit2} " +
                    $"moet er geschoten worden op één van de volgende coordinaten: {expectedCoordinates.ToArray().Print()}. " +
                    $"De strategie kiest soms echter {result}");

                Setup();
            }
        }

        [MonitoredTest("EXTRA - DetermineTargetCoordinate - Should shoot nearby hit squares when 2 ships are hit")]
        public void DetermineTargetCoordinate_ShouldShootNearbyHitSquaresWhen2ShipsAreHit()
        {
            for (int numberOfChecks = 0; numberOfChecks < 10; numberOfChecks++)
            {
                //Arrange
                GridCoordinate hit1 = new GridCoordinate(RandomGenerator.Next(2, _grid.Size - 2), RandomGenerator.Next(2, _grid.Size - 2));
                GridCoordinate miss1 = new GridCoordinate(hit1.Row, hit1.Column - 1);
                GridCoordinate hit2 = new GridCoordinate(hit1.Row, hit1.Column + 1);
                GridCoordinate miss2 = new GridCoordinate(hit2.Row, hit2.Column + 1);
                var ship1 = new ShipBuilder(ShipKind.Carrier).Build();
                var ship2 = new ShipBuilder(ShipKind.Battleship).Build();

                _gridBuilder.WithSquareStatus(miss1, GridSquareStatus.Miss);
                _gridBuilder.WithSquareStatus(hit1, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit1, ShotResult.CreateHit(ship1, true));

                _gridBuilder.WithSquareStatus(hit2, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit2, ShotResult.CreateHit(ship2, true));
                _gridBuilder.WithSquareStatus(miss2, GridSquareStatus.Miss);

                IList<GridCoordinate> expectedCoordinates = new List<GridCoordinate>
                {
                    new GridCoordinate(hit1.Row + 1, hit1.Column),
                    new GridCoordinate(hit1.Row - 1, hit1.Column),
                    new GridCoordinate(hit2.Row + 1, hit2.Column),
                    new GridCoordinate(hit2.Row - 1, hit2.Column),
                };

                //Act
                GridCoordinate result = _strategy.DetermineTargetCoordinate();

                Assert.That(expectedCoordinates.Contains(result), Is.True,
                    $"Bij een grid met volgende situatie: misser op {miss1}, 2 rake schoten shot op {hit1} en {hit2} en een misser op {miss2} " +
                    $"moet er geschoten worden op één van de volgende coordinaten: {expectedCoordinates.ToArray().Print()}. " +
                    $"De strategie kiest soms echter {result}");

                Setup();
            }
        }

        [MonitoredTest("EXTRA - DetermineTargetCoordinate - Should detect the ship with 2 hits when a neighbor ship also has a hit")]
        public void DetermineTargetCoordinate_ShouldDetectTheShipWith2HitsWhenANeighborShipAlsoHasAHit()
        {
            for (int numberOfChecks = 0; numberOfChecks < 10; numberOfChecks++)
            {
                //Arrange
                GridCoordinate hit1 = new GridCoordinate(RandomGenerator.Next(2, _grid.Size - 2), RandomGenerator.Next(2, _grid.Size - 2));
                GridCoordinate miss1 = new GridCoordinate(hit1.Row - 1, hit1.Column);
                GridCoordinate hit2 = new GridCoordinate(hit1.Row + 1, hit1.Column);
                GridCoordinate miss2 = new GridCoordinate(hit2.Row + 1, hit2.Column);
                GridCoordinate hit3 = new GridCoordinate(hit2.Row, hit2.Column + 1);
                var ship1 = new ShipBuilder(ShipKind.Carrier).Build();
                var ship2 = new ShipBuilder(ShipKind.Battleship).Build();

                _gridBuilder.WithSquareStatus(miss1, GridSquareStatus.Miss);

                _gridBuilder.WithSquareStatus(hit1, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit1, ShotResult.CreateHit(ship1, true));

                _gridBuilder.WithSquareStatus(hit2, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit2, ShotResult.CreateHit(ship2, true));

                _gridBuilder.WithSquareStatus(miss2, GridSquareStatus.Miss);

                _gridBuilder.WithSquareStatus(hit3, GridSquareStatus.Hit);
                _strategy.RegisterShotResult(hit3, ShotResult.CreateHit(ship2, true));

                IList<GridCoordinate> expectedCoordinates = new List<GridCoordinate>
                {
                    new GridCoordinate(hit3.Row, hit3.Column + 1),
                    new GridCoordinate(hit2.Row, hit2.Column - 1)
                };

                //Act
                GridCoordinate result = _strategy.DetermineTargetCoordinate();

                Assert.That(expectedCoordinates.Contains(result), Is.True,
                    $"Bij een grid met volgende situatie: missers -> {miss1} en {miss2}, rake schoten -> {hit1}, {hit2} en {hit3} " +
                    $"moet er geschoten worden op één van de volgende coordinaten: {expectedCoordinates.ToArray().Print()}. " +
                    $"De strategie kiest soms echter {result}");

                Setup();
            }
        }

        private IList<GridCoordinate> ParseExpectedCoordinates(string allowedCoordinates)
        {
            IList<GridCoordinate> expectedCoordinates = new List<GridCoordinate>();
            string[] coordinatePairs = allowedCoordinates.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            foreach (string coordinatePair in coordinatePairs)
            {
                expectedCoordinates.Add(new GridCoordinate(int.Parse(coordinatePair.Substring(1, 1)),
                    int.Parse(coordinatePair.Substring(3, 1))));
            }

            return expectedCoordinates;
        }

        private static IEnumerable<TestCaseData> EXTRADetermineTargetCoordinateRandomSunkenShipTestCases
        {
            get
            {
                yield return new TestCaseData(
                    "1 0 1 1 1 0 1 0 1 1\n" +
                    "0 1 1 0 1 1 0 1 0 1\n" +
                    "0 0 0 1 0 1 1 0 1 0\n" +
                    "1 1 1 0 1 0 1 1 0 1\n" +
                    "1 1 0 1 0 1 0 1 1 0\n" +
                    "1 0 1 1 1 1 1 0 1 1\n" +
                    "0 1 0 1 0 1 0 1 1 0\n" +
                    "1 0 1 1 1 1 1 1 0 1\n" +
                    "1 1 0 1 0 1 0 1 1 0\n" +
                    "0 1 1 0 1 0 1 1 0 1",
                    2, "(1,0) (2,0) (2,1) (2,2)",
                    new Dictionary<ShipKind, GridCoordinate[]>());
                yield return new TestCaseData(
                    "0 0 1 2 2 1 1 0 0 1\n" +
                    "0 1 1 0 0 1 0 1 0 1\n" +
                    "1 0 0 1 0 1 1 0 1 0\n" +
                    "0 0 1 0 1 0 1 1 0 1\n" +
                    "0 1 0 1 0 1 0 1 1 0\n" +
                    "1 0 1 1 0 1 1 0 0 1\n" +
                    "0 1 0 1 0 1 0 1 1 0\n" +
                    "0 0 1 1 1 1 1 1 1 1\n" +
                    "1 0 0 1 0 1 1 0 0 0\n" +
                    "0 1 1 0 1 0 1 1 0 1",
                    3, "(4,4) (5,4) (6,4) (8,7) (8,8) (8,9)",
                    new Dictionary<ShipKind, GridCoordinate[]>
                    {
                        {
                            ShipKind.PatrolBoat,
                            new GridCoordinate[] {
                                new GridCoordinate(0, 3),
                                new GridCoordinate(0, 4)
                            }
                        }
                    });
                yield return new TestCaseData(
                    "1 0 0 0 2 0 1 0 1 1\n" +
                    "0 1 1 0 2 1 0 1 0 1\n" +
                    "0 0 0 1 0 1 1 0 1 0\n" +
                    "1 1 1 0 1 2 2 2 0 1\n" +
                    "1 1 0 1 0 1 0 1 1 0\n" +
                    "1 0 1 0 1 1 2 0 1 0\n" +
                    "0 0 0 0 1 0 2 1 1 0\n" +
                    "1 0 1 1 0 0 2 1 0 0\n" +
                    "1 1 0 1 0 1 0 1 1 0\n" +
                    "0 1 1 0 0 0 1 1 0 1",
                    4, "(6,0) (6,1) (6,2) (6,3) (4,9) (5,9) (6,9) (7,9) (8,9)",
                    new Dictionary<ShipKind, GridCoordinate[]>
                    {
                        {
                            ShipKind.PatrolBoat,
                            new GridCoordinate[] {
                                new GridCoordinate(0, 4),
                                new GridCoordinate(1, 5)
                            }
                        },
                        {
                            ShipKind.Destroyer,
                            new GridCoordinate[] {
                                new GridCoordinate(3, 5),
                                new GridCoordinate(3, 6),
                                new GridCoordinate(3, 7)
                            }
                        },
                        {
                            ShipKind.Submarine,
                            new GridCoordinate[] {
                                new GridCoordinate(5, 6),
                                new GridCoordinate(6, 6),
                                new GridCoordinate(7, 6),
                            }
                        }
                    });
            }
        }

        private void RegisterSunkenShip(ShipKind sunkenShipKind, GridCoordinate[] coordinates)
        {
            // Get ship squares
            IGridSquare[] squares = coordinates.Select(c =>
            {
                GridSquare square = new GridSquare(c);
                square.Status = GridSquareStatus.Hit;
                return square;
            }).ToArray();
            var shipBuilder = new ShipBuilder(sunkenShipKind).WithSquares(squares);
            var ship = shipBuilder.Build();

            for (var index = 0; index < ship.Squares.Length - 1; index++)
            {
                IGridSquare shipSquare = ship.Squares[index];
                _strategy.RegisterShotResult(shipSquare.Coordinate, ShotResult.CreateHit(ship, true));
            }

            shipBuilder.WithHasSunk(true);
            _strategy.RegisterShotResult(ship.Squares[ship.Kind.Size - 1].Coordinate, ShotResult.CreateHit(ship, true));
        }

        private void ParseGridConfiguration(string gridConfiguration)
        {
            string[] rows = gridConfiguration.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rows.Length; i++)
            {
                string[] values = rows[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < values.Length; j++)
                {
                    GridSquareStatus status;
                    switch (values[j])
                    {
                        case "1":
                            status = GridSquareStatus.Miss;
                            break;
                        case "2":
                            status = GridSquareStatus.Hit;
                            break;
                        default:
                            status = GridSquareStatus.Untouched;
                            break;
                    }

                    GridCoordinate coordinate = new GridCoordinate(i, j);
                    _gridBuilder.WithSquareStatus(coordinate, status);
                }
            }

            DetectSunkenShipsHorizontally();
            DetectSunkenShipsVertically();

        }

        private void DetectSunkenShipsHorizontally()
        {
            for (int i = 0; i < _grid.Size; i++)
            {
                for (int j = 0; j < _grid.Size; j++)
                {
                    if (_grid.Squares[i, j].Status == GridSquareStatus.Hit)
                    {
                        int leftIndex = j;
                        while (j < _grid.Size && _grid.Squares[i, j].Status == GridSquareStatus.Hit)
                        {
                            j++;
                        }

                        int rightIndex = j - 1;
                        if (rightIndex > leftIndex)
                        {
                            var kind = ShipKind.All.First(kind => kind.Size == rightIndex - leftIndex + 1);
                            var shipBuilder = new ShipBuilder(kind);
                            var ship = shipBuilder.Build();
                            IList<IGridSquare> shipSquares = new List<IGridSquare>();
                            for (var index = leftIndex; index < rightIndex; index++)
                            {
                                shipSquares.Add(_grid.Squares[i, index]);
                                _strategy.RegisterShotResult(_grid.Squares[i, index].Coordinate,
                                    ShotResult.CreateHit(ship, true));
                            }

                            shipSquares.Add(_grid.Squares[i, rightIndex]);
                            shipBuilder.WithSquares(shipSquares.ToArray());
                            shipBuilder.WithHasSunk(true);
                            _strategy.RegisterShotResult(_grid.Squares[i, rightIndex].Coordinate,
                                ShotResult.CreateHit(ship, true));
                        }
                    }
                }
            }
        }

        private void DetectSunkenShipsVertically()
        {
            for (int j = 0; j < _grid.Size; j++)
            {
                for (int i = 0; i < _grid.Size; i++)
                {
                    if (_grid.Squares[i, j].Status == GridSquareStatus.Hit)
                    {
                        int topIndex = i;
                        while (i < _grid.Size && _grid.Squares[i, j].Status == GridSquareStatus.Hit)
                        {
                            i++;
                        }

                        int bottomIndex = i - 1;
                        if (bottomIndex > topIndex)
                        {
                            var kind = ShipKind.All.First(kind => kind.Size == bottomIndex - topIndex + 1);
                            var shipBuilder = new ShipBuilder(kind);
                            var ship = shipBuilder.Build();

                            IList<IGridSquare> shipSquares = new List<IGridSquare>();
                            for (var index = topIndex; index < bottomIndex; index++)
                            {
                                shipSquares.Add(_grid.Squares[index, j]);
                                _strategy.RegisterShotResult(_grid.Squares[index, j].Coordinate,
                                    ShotResult.CreateHit(ship, true));
                            }

                            shipSquares.Add(_grid.Squares[bottomIndex, j]);

                            shipBuilder.WithSquares(shipSquares.ToArray());
                            shipBuilder.WithHasSunk(true);
                            _strategy.RegisterShotResult(_grid.Squares[bottomIndex, j].Coordinate,
                                ShotResult.CreateHit(ship, true));
                        }
                    }
                }
            }
        }
    }
}
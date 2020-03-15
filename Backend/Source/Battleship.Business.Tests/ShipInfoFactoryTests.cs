using System.Collections.Generic;
using System.Linq;
using Battleship.Business.Models;
using Battleship.Business.Models.Contracts;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.TestTools;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Business.Tests
{
    public class ShipInfoFactoryTests : TestBase
    {
        private ShipInfoFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new ShipInfoFactory();
        }

        [MonitoredTest("CreateMultipleFromFleet - Converts all ships of the fleet to ShipInfos")]
        public void CreateMultipleFromFleet_ConvertsAllShipsOfTheFleetToShipInfos()
        {
            //Arrange
            IFleet fleet = ArrangeFleet();

            //Act
            IList<IShipInfo> shipInfos = _factory.CreateMultipleFromFleet(fleet);

            //Assert
            Assert.That(shipInfos, Is.Not.Null, "No list of ship infos is returned.");
            var allShips = fleet.GetAllShips();
            Assert.That(shipInfos.Count, Is.EqualTo(allShips.Count), "The number of ship infos should be equal to the number of ships in the fleet.");

            foreach (IShip ship in allShips)
            {
                AssertShipInfoExistsForShip(ship, shipInfos);
            }
        }

        [MonitoredTest("CreateMultipleFromSunkenShipsOfFleet - Converts all sunken ships of the fleet to ShipInfos")]
        public void CreateMultipleFromSunkenShipsOfFleet_ConvertsAllSunkenShipsOfTheFleetToShipInfos()
        {
            //Arrange
            IFleet fleet = ArrangeFleet();

            //Act
            IList<IShipInfo> shipInfos = _factory.CreateMultipleFromSunkenShipsOfFleet(fleet);

            //Assert
            Assert.That(shipInfos, Is.Not.Null, "No list of ship infos is returned.");
            var allSunkenShips = fleet.GetSunkenShips();
            Assert.That(shipInfos.Count, Is.EqualTo(allSunkenShips.Count), "The number of ship infos should be equal to the number of sunken ships in the fleet.");

            foreach (IShip sunkenShip in allSunkenShips)
            {
                AssertShipInfoExistsForShip(sunkenShip, shipInfos);
            }
        }

        [MonitoredTest("ShipInfo - Constructor - Should copy ship properties")]
        public void ShipInfo_Constructor_ShouldCopyShipProperties()
        {
            //Arrange
            IShip ship = ArrangeShip(ShipKind.All.NextRandomElement(), RandomGenerator.NextBool());

            //Act
            IShipInfo shipInfo = new ShipInfo(ship);

            //Assert
            Assert.That(shipInfo.Kind, Is.EqualTo(ship.Kind), "The Kind is not set correctly.");
            Assert.That(shipInfo.HasSunk, Is.EqualTo(ship.HasSunk), "HasSunk is not set correctly.");
            Assert.That(shipInfo.Coordinates.All(infoCoordinate =>
                    ship.Squares.Any(s => s.Coordinate == infoCoordinate)), Is.True,
                "The Coordinates are not set correctly.");
        }

        [MonitoredTest("ShipInfo - Constructor - Should leave Coordinates empty when ship is not positioned on a grid")]
        public void ShipInfo_Constructor_ShouldLeaveCoordinatesEmptyWhenShipIsNotPositionedOnAGrid()
        {
            //Arrange
            IShip ship = ArrangeShip(ShipKind.All.NextRandomElement(), RandomGenerator.NextBool(), isPositioned: false);

            //Act
            IShipInfo shipInfo = new ShipInfo(ship);

            //Assert
            Assert.That(shipInfo.Coordinates, Is.Not.Null, "Coordinates should not be null. It should be an empty array.");
            Assert.That(shipInfo.Coordinates, Has.Length.Zero, "The Coordinates array is not empty.");
        }

        private IFleet ArrangeFleet()
        {
            var fleetMock = new Mock<IFleet>();
            IList<IShip> allShips = new List<IShip>();
            foreach (var kind in ShipKind.All)
            {
                allShips.Add(ArrangeShip(kind: kind, hasSunk: RandomGenerator.NextBool()));
            }
            fleetMock.Setup(f => f.GetAllShips()).Returns(allShips);
            fleetMock.Setup(f => f.GetSunkenShips()).Returns(allShips.Where(s => s.HasSunk).ToList());
            return fleetMock.Object;
        }

        private IShip ArrangeShip(ShipKind kind, bool hasSunk, bool isPositioned = true)
        {
            var shipMock = new Mock<IShip>();
            shipMock.SetupGet(s => s.HasSunk).Returns(hasSunk);
            shipMock.SetupGet(s => s.Kind).Returns(kind);
            if (isPositioned)
            {
                shipMock.SetupGet(s => s.Squares).Returns(BuildSquares(kind.Size));
            }
            
            return shipMock.Object;
        }

        private GridSquare[] BuildSquares(int numberOfSquares)
        {
            var squares = new GridSquare[numberOfSquares];
            for (int i = 0; i < numberOfSquares; i++)
            {
                squares[i] = new GridSquare(new GridCoordinate(RandomGenerator.Next(1,11), RandomGenerator.Next(1, 11)));
            }

            return squares;
        }

        private static void AssertShipInfoExistsForShip(IShip ship, IList<IShipInfo> shipInfos)
        {
            IShipInfo matchingShipInfo = shipInfos.FirstOrDefault(info => info.Kind == ship.Kind);
            Assert.That(matchingShipInfo, Is.Not.Null, $"No info is found about the '{ship.Kind.Name}' ship.");
            Assert.That(matchingShipInfo.HasSunk, Is.EqualTo(ship.HasSunk),
                $"The HasSunk property of the '{ship.Kind.Name}' ship is not set correctly.");
            Assert.That(
                matchingShipInfo.Coordinates.All(infoCoordinate =>
                    ship.Squares.Any(s => s.Coordinate == infoCoordinate)), Is.True,
                $"The Coordinates of the '{ship.Kind.Name}' ship are not set correctly.");
        }
    }
}
using Battleship.Business.Models;
using Battleship.Business.Models.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.TestTools;
using Guts.Client.Shared;
using Moq;
using NUnit.Framework;

namespace Battleship.Business.Tests
{
    public class GridInfoFactoryTests : TestBase
    {
        private GridInfoFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new GridInfoFactory();
        }

        [MonitoredTest("CreateFromGrid - Converts the square matrix of the grid to a jagged array")]
        public void CreateFromGrid_ConvertsTheSquareMatrixOfTheGridToAJaggedArray()
        {
            //Arrange
            var grid = ArrangeGrid();

            //Act
            IGridInfo gridInfo = _factory.CreateFromGrid(grid);

            //Assert
            Assert.That(gridInfo, Is.Not.Null, "No instance of a class that implements IGridInfo is returned.");
            Assert.That(gridInfo.Size, Is.EqualTo(grid.Size), "The Size should be the same as the Size of the grid.");

            Assert.That(gridInfo.Squares.Length, Is.EqualTo(grid.Size), "The Squares should have a length equal to the size of the grid.");
            foreach (var squareInfoRow in gridInfo.Squares)
            {
                Assert.That(squareInfoRow.Length, Is.EqualTo(grid.Size),
                    "Each row of squares should have a length equal to the size of the grid.");
            }
        }

        [MonitoredTest("CreateFromGrid - Generates the correct GridSquareInfos")]
        public void CreateFromGrid_GeneratesTheCorrectGridSquareInfos()
        {
            //Arrange
            var grid = ArrangeGrid();

            //Act
            IGridInfo gridInfo = _factory.CreateFromGrid(grid);

            //Assert
            Assert.That(gridInfo, Is.Not.Null, "No instance of a class that implements IGridInfo is returned.");
         
            for (var i = 0; i < gridInfo.Squares.Length; i++)
            {
                GridSquareInfo[] squareInfoRow = gridInfo.Squares[i];
                for (var j = 0; j < squareInfoRow.Length; j++)
                {
                    GridSquareInfo squareInfo = squareInfoRow[j];
                    IGridSquare matchingSquare = grid.Squares[i, j];

                    Assert.That(squareInfo.NumberOfBombs, Is.EqualTo(matchingSquare.NumberOfBombs),
                        $"Number of bombs for square info at [{i}][{j}] is not correct.");
                    Assert.That(squareInfo.Status, Is.EqualTo(matchingSquare.Status),
                        $"Status for square info at [{i}][{j}] is not correct.");
                }
            }
        }

        private IGrid ArrangeGrid()
        {
            var gridMock = new Mock<IGrid>();
            int size = RandomGenerator.Next(5,11);
            gridMock.SetupGet(g => g.Size).Returns(size);
            var squares = new GridSquare[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var square = new GridSquare(new GridCoordinate(i, j));
                    int numberOfHits = RandomGenerator.Next(0, 3);
                    for (int k = 0; k < numberOfHits; k++)
                    {
                        square.HitByBomb();
                    }
                    squares[i, j] = square;
                }
            }
            gridMock.SetupGet(g => g.Squares).Returns(squares);
            return gridMock.Object;
        }
    }
}
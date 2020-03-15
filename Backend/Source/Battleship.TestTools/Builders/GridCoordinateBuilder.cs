using System;
using Battleship.Domain.GridDomain;

namespace Battleship.TestTools.Builders
{
    public class GridCoordinateBuilder
    {
        private static readonly Random RandomGenerator = new Random();

        private readonly GridCoordinate _coordinate;

        public GridCoordinateBuilder(int gridSize = 10)
        {
            int row = RandomGenerator.Next(0, gridSize);
            int column = RandomGenerator.Next(0, gridSize);
            _coordinate = new GridCoordinate(row, column);
        }

        public GridCoordinate Build()
        {
            return _coordinate;
        }
    }
}
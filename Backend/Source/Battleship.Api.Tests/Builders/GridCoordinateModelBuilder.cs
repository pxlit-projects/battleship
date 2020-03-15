using System;
using Battleship.Api.Models;

namespace Battleship.Api.Tests.Builders
{
    public class GridCoordinateModelBuilder
    {
        private static readonly Random RandomGenerator = new Random();

        private readonly GridCoordinateModel _model;

        public GridCoordinateModelBuilder()
        {
            _model = new GridCoordinateModel
            {
                Row = RandomGenerator.Next(1, 11),
                Column = RandomGenerator.Next(1, 11)
            };
        }

        public GridCoordinateModel Build()
        {
            return _model;
        }
    }
}
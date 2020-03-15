using Battleship.Api.Models;
using Battleship.Domain.FleetDomain;
using Battleship.TestTools;

namespace Battleship.Api.Tests.Builders
{
    public class ShipPositioningModelBuilder
    {
        private readonly ShipPositioningModel _model;

        public ShipPositioningModelBuilder()
        {
            ShipKind kind = ShipKind.All.NextRandomElement();
            _model = new ShipPositioningModel
            {
                ShipCode = kind.Code,
                SegmentCoordinates = new GridCoordinateModel[kind.Size]
            };

            for (int i = 0; i < kind.Size; i++)
            {
                _model.SegmentCoordinates[i] = new GridCoordinateModelBuilder().Build();
            }
        }

        public ShipPositioningModel Build()
        {
            return _model;
        }
    }
}
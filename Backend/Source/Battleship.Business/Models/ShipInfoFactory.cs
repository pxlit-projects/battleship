using System;
using System.Collections.Generic;
using Battleship.Business.Models.Contracts;
using Battleship.Domain.FleetDomain.Contracts;

namespace Battleship.Business.Models
{
    public class ShipInfoFactory : IShipInfoFactory 
    {
        public IList<IShipInfo> CreateMultipleFromFleet(IFleet fleet)
        {
            throw new NotImplementedException("CreateMultipleFromFleet of ShipInfoFactory class is not implemented");
        }

        public IList<IShipInfo> CreateMultipleFromSunkenShipsOfFleet(IFleet fleet)
        {
            throw new NotImplementedException("CreateMultipleFromSunkenShipsOfFleet of ShipInfoFactory class is not implemented");
        }
    }
}
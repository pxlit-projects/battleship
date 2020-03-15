using System.Collections.Generic;
using Battleship.Domain.FleetDomain.Contracts;

namespace Battleship.Business.Models.Contracts
{
    public interface IShipInfoFactory
    {
        IList<IShipInfo> CreateMultipleFromFleet(IFleet fleet);
        IList<IShipInfo> CreateMultipleFromSunkenShipsOfFleet(IFleet fleet);
    }
}
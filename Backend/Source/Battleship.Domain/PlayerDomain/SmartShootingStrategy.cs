using System;
using Battleship.Domain.GridDomain;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.PlayerDomain
{
    public class SmartShootingStrategy : IShootingStrategy
    {
        public GridCoordinate DetermineTargetCoordinate()
        {
            throw new NotImplementedException();
        }

        public void RegisterShotResult(ShotResult shotResult)
        {
            throw new NotImplementedException();
        }
    }
}
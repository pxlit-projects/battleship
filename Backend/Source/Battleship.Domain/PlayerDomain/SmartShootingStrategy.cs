using System;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.PlayerDomain
{
    public class SmartShootingStrategy : IShootingStrategy
    {
        public SmartShootingStrategy(GameSettings settings, IGrid opponentGrid)
        {

        }

        public GridCoordinate DetermineTargetCoordinate()
        {
            throw new NotImplementedException();
        }

        public void RegisterShotResult(GridCoordinate target, ShotResult shotResult)
        {
            throw new NotImplementedException();
        }
    }
}
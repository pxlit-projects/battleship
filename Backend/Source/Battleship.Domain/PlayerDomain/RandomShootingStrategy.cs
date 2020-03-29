using System;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.PlayerDomain
{
    public class RandomShootingStrategy : IShootingStrategy
    {
        public RandomShootingStrategy(GameSettings settings, IGrid opponentGrid)
        {
            //The GameSettings parameter will only be needed when you implement certain extra's. But you must leave it. Otherwise some tests will not compile...
        }

        public GridCoordinate DetermineTargetCoordinate()
        {
            throw new NotImplementedException("DetermineTargetCoordinate method of RandomShootingStrategy class is not implemented");
        }

        public void RegisterShotResult(GridCoordinate target, ShotResult shotResult)
        {
            //No need do do anything here. Smarter shooting strategies will care more about the result of a shot...
        }
    }
}
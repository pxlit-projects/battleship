using System;
using Battleship.Domain.GameDomain;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.PlayerDomain
{
    public class ComputerPlayer : PlayerBase
    {

        public ComputerPlayer(GameSettings settings, IShootingStrategy shootingStrategy) : base(Guid.NewGuid(), "Computer", settings)
        {
        }

        public void ShootAutomatically(IPlayer opponent)
        {
            throw new NotImplementedException("ShootAutomatically method of ComputerPlayer class is not implemented");
        }
    }
}
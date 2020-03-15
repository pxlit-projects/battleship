using System;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.PlayerDomain
{
    public abstract class PlayerBase : IPlayer
    {
        public Guid Id { get; }
        public string NickName { get; }
        public IGrid Grid { get; }
        public IFleet Fleet { get; }

        public bool HasBombsLoaded { get; }

        protected PlayerBase(Guid id, string nickName, GameSettings gameSettings)
        {
        }

        public void ReloadBombs()
        {
            throw new NotImplementedException("ReloadBombs method of PlayerBase class is not implemented");
        }

        public ShotResult ShootAt(IPlayer opponent, GridCoordinate coordinate)
        {
            throw new NotImplementedException("ShootAt method of PlayerBase class is not implemented");
        }
    }
}
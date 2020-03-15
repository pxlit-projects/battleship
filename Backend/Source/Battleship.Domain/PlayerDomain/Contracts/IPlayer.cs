using System;
using Battleship.Domain.FleetDomain.Contracts;
using Battleship.Domain.GridDomain;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Domain.PlayerDomain.Contracts
{
    public interface IPlayer
    {
        Guid Id { get; }
        string NickName { get; }
        IGrid Grid { get; }
        IFleet Fleet { get; }
        bool HasBombsLoaded { get; }

        void ReloadBombs();
        ShotResult ShootAt(IPlayer opponent, GridCoordinate coordinate);
    }
}
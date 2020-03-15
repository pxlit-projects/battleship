using System;
using Battleship.Domain.GridDomain;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.GameDomain.Contracts
{
    public interface IGame
    {
        Guid Id { get; }
        GameSettings Settings { get; }
        IPlayer Player1 { get; }
        IPlayer Player2 { get; }
        bool IsStarted { get; }

        Result Start();
        ShotResult ShootAtOpponent(Guid shooterPlayerId, GridCoordinate targetCoordinate);
        IPlayer GetPlayerById(Guid playerId);
        IPlayer GetOpponent(IPlayer player);
    }
}
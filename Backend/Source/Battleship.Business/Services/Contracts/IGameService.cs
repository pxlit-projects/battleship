using System;
using Battleship.Business.Models.Contracts;
using Battleship.Domain;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;

namespace Battleship.Business.Services.Contracts
{
    public interface IGameService
    {
        IGameInfo CreateGameForUser(GameSettings settings, User user);
        Result StartGame(Guid gameId, Guid playerId);
        IGameInfo GetGameInfoForPlayer(Guid gameId, Guid playerId);
        Result PositionShipOnGrid(Guid gameId, Guid playerId, ShipKind shipKind, GridCoordinate[] segmentCoordinates);
        ShotResult ShootAtOpponent(Guid gameId, Guid shooterPlayerId, GridCoordinate coordinate);
       
    }
}
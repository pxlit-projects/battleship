using System;
using Battleship.Domain.GameDomain.Contracts;

namespace Battleship.Business.Models.Contracts
{
    public interface IGameInfoFactory
    {
        IGameInfo CreateFromGame(IGame game, Guid playerId);
    }
}
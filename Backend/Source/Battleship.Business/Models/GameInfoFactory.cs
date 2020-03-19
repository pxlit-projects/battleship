using System;
using Battleship.Business.Models.Contracts;
using Battleship.Domain.GameDomain.Contracts;

namespace Battleship.Business.Models
{
    public class GameInfoFactory : IGameInfoFactory
    {
        public GameInfoFactory(IGridInfoFactory gridInfoFactory, IShipInfoFactory shipInfoFactory)
        {
        }

        public IGameInfo CreateFromGame(IGame game, Guid playerId)
        {
            throw new NotImplementedException("CreateFromGame method of GameInfoFactory class is not implemented");
        }
    }
}
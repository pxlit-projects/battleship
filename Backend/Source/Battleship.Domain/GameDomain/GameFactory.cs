using System;
using Battleship.Domain.GameDomain.Contracts;
using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.GameDomain
{
    public class GameFactory : IGameFactory
    {
        public IGame CreateNewSinglePlayerGame(GameSettings settings, User user)
        {
            throw new NotImplementedException("CreateNewSinglePlayerGame method of GameFactory class is not implemented");
        }

        public IGame CreateNewTwoPlayerGame(GameSettings settings, IPlayer player1, IPlayer player2)
        {
            //This only needs to be implemented when you add the extra of multiplayer games
            throw new System.NotImplementedException();
        }
    }
}
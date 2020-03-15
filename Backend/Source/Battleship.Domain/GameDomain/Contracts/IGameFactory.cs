using Battleship.Domain.PlayerDomain.Contracts;

namespace Battleship.Domain.GameDomain.Contracts
{
    public interface IGameFactory
    {
        IGame CreateNewSinglePlayerGame(GameSettings settings, User user);

        IGame CreateNewTwoPlayerGame(GameSettings settings, IPlayer player1, IPlayer player2);
    }
}
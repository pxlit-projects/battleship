using Battleship.Domain.GameDomain;

namespace Battleship.Domain.PlayerDomain
{
    public class HumanPlayer : PlayerBase
    {
        public HumanPlayer(User user, GameSettings gameSettings) : base(user.Id, user.NickName, gameSettings)
        {
            
        }
    }
}
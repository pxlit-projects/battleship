using Battleship.Domain.GridDomain;

namespace Battleship.Domain.PlayerDomain.Contracts
{
    public interface IShootingStrategy
    {
        /// <summary>
        /// Tries to make an educated guess where to shoot next.
        /// </summary>
        GridCoordinate DetermineTargetCoordinate();

        /// <summary>
        /// After a shot is fired (by a computer player)
        /// it should let the shooting strategy know what the result was
        /// so it can make a better judgment on where to shoot next.
        /// </summary>
        void RegisterShotResult(ShotResult shotResult);
    }
}
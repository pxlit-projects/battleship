using System;
using System.Collections.Generic;

namespace Battleship.Business.Models.Contracts
{
    public interface IGameInfo
    {
        /// <summary>
        /// Unique identifier of the game.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Indicates if both players have their whole fleet (5 ships) positioned on the grid.
        /// </summary>
        bool IsReadyToStart { get; }

        /// <summary>
        /// Indicates if the player can shoot.
        /// If the player has no bombs loaded, then he must wait for the opponent to shoot his bomb(s)
        /// </summary>
        bool HasBombsLoaded { get; }

        /// <summary>
        /// The grid of the player
        /// </summary>
        IGridInfo OwnGrid { get; }

        /// <summary>
        /// All the ships of the player
        /// </summary>
        IList<IShipInfo> OwnShips { get; }

        /// <summary>
        /// The grid of the opponent
        /// </summary>
        IGridInfo OpponentGrid { get; }

        /// <summary>
        /// The ships of the opponent that are known to be sunken.
        /// When the game settings indicate that it is not required to report when a ship has sunk, then this list will always be empty.
        /// </summary>
        IList<IShipInfo> SunkenOpponentShips { get; }
    }
}
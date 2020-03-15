using System;

namespace Battleship.Domain.GameDomain
{
    public class GameSettings
    {
        /// <summary>
        /// Size of the grid.
        /// Must be a value between 10 and 15 (10 and 15 included)
        /// Default value = 10.
        /// </summary>
        public int GridSize { get; set; }

        /// <summary>
        /// Indicates if it is allowed to have the segments of a ship to not be aligned vertically or horizontally.
        /// If deformed ships are allowed the segments of a ship may also touch diagonally.
        /// Default value = false.
        /// </summary>
        public bool AllowDeformedShips { get; set; }

        /// <summary>
        /// There are 4 game modes:
        /// 1 = Default: the classic mode in which each player can shoot one bomb per turn.
        /// 2 = MultipleShotsPerTurnConstant: each player can shoot exactly 5 bombs per turn.
        /// 3 = MultipleShotsPerTurnBiggestUndamagedShip: the number of bombs that a player can shoot in one turn is equal to the size of the biggest undamaged ship (with a minimum of 1 bomb).
        /// 4 = MultipleShotsPerTurnNumberOfShips: the number of bombs that a player can shoot in one turn is equal to the number of remaining ships.
        ///
        /// Default value = 1;
        /// </summary>
        public GameMode Mode { get; set; }

        /// <summary>
        /// Indicates if the opponent must let the player know if a shot of the player sunk a whole ship of the opponent.
        /// Default value = true;
        /// </summary>
        public bool MustReportSunkenShip { get; set; }

        /// <summary>
        /// Indicates if a ships can be moved during the game.
        /// Only undamaged ships can be moved.
        /// If true, the <see cref="NumberOfTurnsBeforeAShipCanBeMoved"/> determined how many turns a player must wait before he can move a ship again.
        ///
        /// Default value = false.
        /// </summary>
        public bool CanMoveUndamagedShipsDuringGame { get; set; }

        /// <summary>
        /// The number of turns a player must wait before het can move a ship.
        /// Must be a value between 1 and 10 (1 and 10 included)
        /// This property is only relevant when <see cref="CanMoveUndamagedShipsDuringGame"/> is true.
        ///
        /// Default value = 5.
        /// </summary>
        public int NumberOfTurnsBeforeAShipCanBeMoved { get; set; }

        public GameSettings()
        {
            throw new NotImplementedException("Constructor of GameSettings class is not implemented");
        }
    }
}
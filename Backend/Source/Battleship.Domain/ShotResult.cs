using Battleship.Domain.FleetDomain;
using Battleship.Domain.FleetDomain.Contracts;

namespace Battleship.Domain
{
    //DO NOT TOUCH THIS FILE!

    /// <summary>
    /// Contains feedback on a shot at the opponent.
    /// </summary>
    public class ShotResult
    {
        /// <summary>
        /// Indicates if the shot actually succeeded.
        /// If false, the <see cref="MisfireReason"/> will contain the reason the shot did not succeed
        /// (e.g. No bombs loaded, the opponent must shoot first).
        /// </summary>
        public bool ShotFired { get; }

        /// <summary>
        /// Indicates if the bomb hit a segment of an opponent ship.
        /// </summary>
        public bool Hit { get; }

        /// <summary>
        /// When the shot causes the opponents ship to sink, then the kind of sunken ship will be reported here
        /// (if the game settings dictate that sunken ships must be reported).
        /// When it is not required to report sunken ships or the shot did not cause a ship to sink, then the value will be null.
        /// </summary>
        public ShipKind SunkenShipKind { get; private set; }

        /// <summary>
        /// When <see cref="ShotFired"/> is false, this will contain the reason why the shot did not fired.
        /// </summary>
        public string MisfireReason { get; private set; }


        private ShotResult(bool shotFired, bool hit)
        {
            ShotFired = shotFired;
            Hit = hit;
            SunkenShipKind = null;
            MisfireReason = string.Empty;
        }

        public static ShotResult CreateMissed()
        {
            return new ShotResult(true,false);
        }

        public static ShotResult CreateHit(IShip ship, bool reportSunkenShip = false)
        {
            var result = new ShotResult(true,true);
            if (ship.HasSunk && reportSunkenShip)
            {
                result.SunkenShipKind = ship.Kind;
            }

            return result;
        }

        public static ShotResult CreateMisfire(string reason)
        {
            var result = new ShotResult(false, false) {MisfireReason = reason};

            return result;
        }
    }
}
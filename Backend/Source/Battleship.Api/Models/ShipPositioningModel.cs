namespace Battleship.Api.Models
{
    public class ShipPositioningModel
    {
        /// <summary>
        /// The code of the ship.
        /// CAR = Carrier
        /// BS = Battleship
        /// DS = Destroyer
        /// SM = Submarine
        /// PB = Patrol boat
        /// </summary>
        public string ShipCode { get; set; }

        /// <summary>
        /// The coordinate on the grid for each segment of the ship. (E.g. a Carrier has 5 segments)
        /// </summary>
        public GridCoordinateModel[] SegmentCoordinates { get; set; }
    }
}
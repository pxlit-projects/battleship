using System;
using System.Collections.Generic;
using Battleship.Business.Models.Contracts;

namespace Battleship.Business.Models
{
    public class GameInfo : IGameInfo
    {
        public Guid Id { get; set; }

        public bool IsReadyToStart { get; set; }

        public bool HasBombsLoaded { get; set; }

        public IGridInfo OwnGrid { get; set; }

        public IList<IShipInfo> OwnShips { get; set; }

        public IGridInfo OpponentGrid { get; set; }

        public IList<IShipInfo> SunkenOpponentShips { get; set; }
    }
}
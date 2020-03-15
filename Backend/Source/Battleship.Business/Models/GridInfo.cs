using Battleship.Business.Models.Contracts;

namespace Battleship.Business.Models
{
    public class GridInfo : IGridInfo
    {
        public GridSquareInfo[][] Squares { get; internal set; } //must use jagged array because this is the only way to be able to convert it to json correctly

        public int Size { get; internal set; }
    }
}
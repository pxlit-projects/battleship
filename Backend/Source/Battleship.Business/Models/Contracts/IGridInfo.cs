namespace Battleship.Business.Models.Contracts
{
    public interface IGridInfo
    {
        GridSquareInfo[][] Squares { get; }
        int Size { get; }
    }
}
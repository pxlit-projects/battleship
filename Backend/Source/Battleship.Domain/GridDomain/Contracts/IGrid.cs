namespace Battleship.Domain.GridDomain.Contracts
{
    public interface IGrid
    {
        IGridSquare[,] Squares { get; }
        int Size { get; }
        IGridSquare GetSquareAt(GridCoordinate coordinate);
        IGridSquare Shoot(GridCoordinate coordinate);
    }
}
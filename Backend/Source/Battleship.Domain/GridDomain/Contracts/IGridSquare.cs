namespace Battleship.Domain.GridDomain.Contracts
{
    public interface IGridSquare
    {
        GridSquareStatus Status { get; set; }
        GridCoordinate Coordinate { get; }
        int NumberOfBombs { get; }
        event HitByBombHandler OnHitByBomb;
        void HitByBomb();
    }
}
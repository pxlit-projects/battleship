namespace Battleship.Domain.GameDomain
{
    public enum GameMode
    {
        Default = 1,
        MultipleShotsPerTurnConstant = 2, //5 shots per turn
        MultipleShotsPerTurnBiggestUndamagedShip = 3, //Number of shots in one turn is equal to the size of the biggest undamaged ship (with a minimum of 1 shot)
        MultipleShotsPerTurnNumberOfShips = 4 //Number of shots in one turn is equal to the remaining ships
    }
}
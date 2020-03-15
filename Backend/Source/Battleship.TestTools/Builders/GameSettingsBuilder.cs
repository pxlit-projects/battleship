using System;
using Battleship.Domain.GameDomain;

namespace Battleship.TestTools.Builders
{
    public class GameSettingsBuilder
    {
        private static readonly Random RandomGenerator = new Random();

        private readonly GameSettings _settings;

        public GameSettingsBuilder()
        {
            _settings = new GameSettings
            {
                AllowDeformedShips = false,
                CanMoveUndamagedShipsDuringGame = false,
                GridSize = RandomGenerator.Next(10,16),
                Mode = GameMode.Default,
                MustReportSunkenShip = true,
                NumberOfTurnsBeforeAShipCanBeMoved = RandomGenerator.Next(1,11)
            };
        }

        public GameSettingsBuilder WithMustReportSunkenShip(bool mustReportSunkenShip)
        {
            _settings.MustReportSunkenShip = mustReportSunkenShip;
            return this;
        }

        public GameSettings Build()
        {
            return _settings;
        }
    }
}
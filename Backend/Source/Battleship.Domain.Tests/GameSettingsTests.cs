using System;
using Battleship.Domain.GameDomain;
using Battleship.TestTools;
using Guts.Client.Shared;
using NUnit.Framework;

namespace Battleship.Domain.Tests
{
    public class GameSettingsTests : TestBase
    {
        private GameSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new GameSettings();
        }

        [MonitoredTest("Constructor - Should set defaults")]
        public void Constructor_ShouldSetDefaults()
        {
            //Assert
            Assert.That(_settings.GridSize, Is.EqualTo(10));
            Assert.That(_settings.AllowDeformedShips, Is.False);
            Assert.That(_settings.Mode, Is.EqualTo(GameMode.Default));
            Assert.That(_settings.MustReportSunkenShip, Is.True);
            Assert.That(_settings.CanMoveUndamagedShipsDuringGame, Is.False);
            Assert.That(_settings.NumberOfTurnsBeforeAShipCanBeMoved, Is.EqualTo(5));
        }

        [MonitoredTest("GridSize - Can only contain values from 10 to 15")]
        public void GridSize_CanOnlyContainValuesFrom10To15()
        {
            //Act + Assert
            ActAndAssertSettingInvalidGridSize(9);
            ActAndAssertSettingInvalidGridSize(16);
            ActAndAssertSettingInvalidGridSize(RandomGenerator.Next(int.MinValue, 9));
            ActAndAssertSettingInvalidGridSize(RandomGenerator.Next(17, int.MaxValue));

            _settings.GridSize = 10;
            _settings.GridSize = 15;
            _settings.GridSize = RandomGenerator.Next(11,15);
        }

        [Test]
        public void EXTRA_NumberOfTurnsBeforeAShipCanBeMoved_CanOnlyContainValuesFrom1To10()
        {
            //Act + Assert
            ActAndAssertSettingInvalidNumberOfTurnsBeforeAShipCanBeMoved(0);
            ActAndAssertSettingInvalidNumberOfTurnsBeforeAShipCanBeMoved(11);
            ActAndAssertSettingInvalidNumberOfTurnsBeforeAShipCanBeMoved(RandomGenerator.Next(int.MinValue, 0));
            ActAndAssertSettingInvalidNumberOfTurnsBeforeAShipCanBeMoved(RandomGenerator.Next(12, int.MaxValue));

            _settings.NumberOfTurnsBeforeAShipCanBeMoved = 1;
            _settings.NumberOfTurnsBeforeAShipCanBeMoved = 10;
            _settings.NumberOfTurnsBeforeAShipCanBeMoved = RandomGenerator.Next(2, 10);
        }

        private void ActAndAssertSettingInvalidGridSize(int invalidSize)
        {
            Assert.That(() => _settings.GridSize = invalidSize, Throws.InstanceOf<ArgumentOutOfRangeException>(),
                $"Setting GridSize to {invalidSize} should throw an ArgumentOutOfRangeException");
        }

        private void ActAndAssertSettingInvalidNumberOfTurnsBeforeAShipCanBeMoved(int invalidNumberOfTurnsBeforeAShipCanBeMoved)
        {
            Assert.That(() => _settings.NumberOfTurnsBeforeAShipCanBeMoved = invalidNumberOfTurnsBeforeAShipCanBeMoved, Throws.InstanceOf<ArgumentOutOfRangeException>(),
                $"Setting NumberOfTurnsBeforeAShipCanBeMoved to {invalidNumberOfTurnsBeforeAShipCanBeMoved} should throw an ArgumentOutOfRangeException");
        }
    }
}
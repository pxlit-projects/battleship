using System;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GameDomain.Contracts;
using Battleship.Domain.PlayerDomain.Contracts;
using Moq;

namespace Battleship.TestTools.Builders
{
    public class GameBuilder
    {
        private readonly Mock<IGame> _gameMock;

        public GameBuilder()
        {
            _gameMock = new Mock<IGame>();
            _gameMock.SetupGet(g => g.Id).Returns(Guid.NewGuid());
            _gameMock.SetupGet(g => g.Settings).Returns(new GameSettingsBuilder().Build());

            IPlayer player1 = new PlayerBuilder().Build();
            IPlayer player2 = new PlayerBuilder().Build();
            WithPlayers(player1, player2);
        }

        public GameBuilder WithPlayers(IPlayer player1, IPlayer player2)
        {
            _gameMock.SetupGet(g => g.Player1).Returns(player1);
            _gameMock.SetupGet(g => g.Player2).Returns(player2);
            _gameMock.Setup(g => g.GetPlayerById(player1.Id)).Returns(player1);
            _gameMock.Setup(g => g.GetPlayerById(player2.Id)).Returns(player2);
            _gameMock.Setup(g => g.GetOpponent(player1)).Returns(player2);
            _gameMock.Setup(g => g.GetOpponent(player2)).Returns(player1);
            return this;
        }

        public GameBuilder WithSettings(GameSettings settings)
        {
            _gameMock.SetupGet(g => g.Settings).Returns(settings);
            return this;
        }

        public Mock<IGame> BuildMock()
        {
            return _gameMock;
        }

        public IGame Build()
        {
            return _gameMock.Object;
        }
    }
}
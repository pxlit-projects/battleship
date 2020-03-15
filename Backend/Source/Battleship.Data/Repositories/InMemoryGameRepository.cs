using System;
using System.Collections.Concurrent;
using Battleship.Business.Services.Contracts;
using Battleship.Domain;
using Battleship.Domain.GameDomain.Contracts;

namespace Battleship.Data.Repositories
{
    public class InMemoryGameRepository : IGameRepository
    {
        private readonly ConcurrentDictionary<Guid, IGame> _gamesDictionary;

        public InMemoryGameRepository()
        {
            _gamesDictionary = new ConcurrentDictionary<Guid, IGame>();
        }

        public IGame Add(IGame newGame)
        {
            _gamesDictionary.TryAdd(newGame.Id, newGame);
            return newGame;
        }

        public IGame GetById(Guid id)
        {
            if (_gamesDictionary.TryGetValue(id, out IGame game))
            {
                return game;
            }

            throw new DataNotFoundException();
        }

        public void DeleteById(Guid id)
        {
            _gamesDictionary.TryRemove(id, out IGame removedGame);
        }
    }
}
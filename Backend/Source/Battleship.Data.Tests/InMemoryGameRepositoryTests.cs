using System;
using System.Collections.Concurrent;
using System.Linq;
using Battleship.Data.Repositories;
using Battleship.Domain;
using Battleship.Domain.GameDomain.Contracts;
using Battleship.TestTools;
using Battleship.TestTools.Builders;
using Guts.Client.Shared;
using Guts.Client.Shared.TestTools;
using NUnit.Framework;

namespace Battleship.Data.Tests
{
    public class InMemoryGameRepositoryTests : TestBase
    {
        private InMemoryGameRepository _repo;
        private ConcurrentDictionary<Guid, IGame> _internalDictionary;

        [SetUp]
        public void SetUp()
        {
            _repo = new InMemoryGameRepository();
            if (_repo.HasPrivateField<ConcurrentDictionary<Guid, IGame>>())
            {
                _internalDictionary = _repo.GetPrivateFieldValue<ConcurrentDictionary<Guid, IGame>>();
            }
        }

        [MonitoredTest("Should use a ConcurrentDictionary for internal storage")]
        public void ShouldUseAConcurrentDictionaryForInternalStorage()
        {
            AssertHasInternalDictionary();
        }

        [MonitoredTest("Add - Should assign an Id to the game and return the saved game")]
        public void Add_ShouldAssignAnIdToTheGameAndReturnTheSavedGame()
        {
            //Arrange
            var newGame = new GameBuilder().Build();

            //Act
            var result = _repo.Add(newGame);

            //Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result, Is.SameAs(newGame));
        }

        [MonitoredTest("GetById - Should return an existing game")]
        public void GetById_ShouldReturnAnExistingGame()
        {
            AssertHasInternalDictionary();

            //Arrange
            var existingGame = AddExistingGameToInternalDictionary();

            //Act
            var result = _repo.GetById(existingGame.Id);

            //Assert
            Assert.That(result, Is.SameAs(result));
        }

        [MonitoredTest("GetById - Should throw DataNotFoundException when the game does not exist")]
        public void GetById_ShouldThrowDataNotFoundExceptionWhenTheGameDoesNotExist()
        {
            //Arrange
            var numberOfGames = new Random().Next(3, 10);
            for (int i = 0; i < numberOfGames; i++)
            {
                AddExistingGameToInternalDictionary();
            }

            //Act + Assert
            Assert.That(() => _repo.GetById(Guid.NewGuid()), Throws.TypeOf<DataNotFoundException>());
        }

        [MonitoredTest("DeleteById - Should remove the game with matching Id")]
        public void DeleteById_ShouldRemoveTheGameWithMatchingId()
        {
            AssertHasInternalDictionary();

            //Arrange
            AddExistingGamesToInternalDictionary(new Random().Next(1, 5));
            var gameToDelete = AddExistingGameToInternalDictionary();
            AddExistingGamesToInternalDictionary(new Random().Next(1, 5));

            //Act
            _repo.DeleteById(gameToDelete.Id);

            //Assert
            Assert.That(_internalDictionary.Values.All(game => game.Id != gameToDelete.Id), Is.True);
        }

        [MonitoredTest("DeleteById - Should do nothing if the game does not exist")]
        public void DeleteById_ShouldDoNothingIfTheGameDoesNotExist()
        {
            AssertHasInternalDictionary();

            //Arrange
            var numberOfGames = new Random().Next(3, 10);
            AddExistingGamesToInternalDictionary(numberOfGames);

            //Act
            _repo.DeleteById(Guid.NewGuid());

            //Assert
            Assert.That(_internalDictionary.Count, Is.EqualTo(numberOfGames));
        }

        private void AssertHasInternalDictionary()
        {
            Assert.That(_internalDictionary, Is.Not.Null);
            Assert.That(_internalDictionary.Count, Is.Zero);
        }

        private IGame AddExistingGameToInternalDictionary()
        {
            var game = new GameBuilder().Build();
            _internalDictionary.TryAdd(game.Id, game);
            return game;
        }

        private void AddExistingGamesToInternalDictionary(int numberOfGames)
        {
            for (int i = 0; i < numberOfGames; i++)
            {
                AddExistingGameToInternalDictionary();
            }
        }
    }
}
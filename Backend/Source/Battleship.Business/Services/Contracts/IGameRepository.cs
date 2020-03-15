using System;
using Battleship.Domain.GameDomain.Contracts;

namespace Battleship.Business.Services.Contracts
{
    public interface IGameRepository
    {
        IGame Add(IGame newGame);
        IGame GetById(Guid id);
        void DeleteById(Guid id);
    }
}
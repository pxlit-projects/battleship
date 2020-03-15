using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Business.Models.Contracts
{
    public interface IGridInfoFactory
    {
        IGridInfo CreateFromGrid(IGrid grid);
    }
}
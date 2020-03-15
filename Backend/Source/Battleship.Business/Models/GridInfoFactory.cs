using System;
using Battleship.Business.Models.Contracts;
using Battleship.Domain.GridDomain.Contracts;

namespace Battleship.Business.Models
{
    public class GridInfoFactory : IGridInfoFactory
    {
        public IGridInfo CreateFromGrid(IGrid grid)
        {
            throw new NotImplementedException("CreateFromGrid method of GridInfoFactory class is not implemented");
        }
    }
}
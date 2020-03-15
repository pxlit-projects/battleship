using System.Collections.Generic;
using System.Security.Claims;
using Battleship.Domain;

namespace Battleship.Business.Services.Contracts
{
    public interface ITokenFactory
    {
        string CreateToken(User user, IList<Claim> currentUserClaims);
    }
}

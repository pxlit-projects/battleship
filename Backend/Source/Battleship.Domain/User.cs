using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Battleship.Domain
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        public string NickName { get; set; }
    }
}

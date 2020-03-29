using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Battleship.Domain
{
    //DO NOT TOUCH THIS FILE!
    public class User : IdentityUser<Guid>
    {
        [Required]
        public string NickName { get; set; }
    }
}

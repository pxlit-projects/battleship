using System;
using Battleship.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Data
{
    //DO NOT TOUCH THIS FILE!!
    public class BattleshipContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public BattleshipContext()
        {
        }

        public BattleshipContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BattleshipDb;Integrated Security=True";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}

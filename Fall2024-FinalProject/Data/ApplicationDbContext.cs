using Fall2024_FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Fall2024_FinalProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bet> Bets { get; set; } = default!;
        public DbSet<Fight> Fights { get; set; } = default!;
        public DbSet<Fighter> Fighters { get; set; } = default!;
        public DbSet<FighterFight> FighterFights { get; set; } = default!;
        public DbSet<Fall2024_FinalProject.Models.FighterFight> FighterFight { get; set; } = default!;
    }
}

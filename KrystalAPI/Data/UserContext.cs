using KrystalAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace KrystalAPI.Data
{
    public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        public DbSet<UserInformation> UserInformation { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Info)
                .WithOne(ui => ui.User)
                .HasForeignKey<UserInformation>(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(tk => tk.User)              
                .WithMany()                  
                .HasForeignKey(rt => rt.UserId);
        }
    }
}

using BankSystem.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BankAccount> BankAccount { get; set; }

        public DbSet<Transaction> Transaction { get; set; }

        public DbSet<Loan> Loan { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().Ignore(c => c.AccessFailedCount)
                                             .Ignore(c => c.LockoutEnabled)
                                             .Ignore(c => c.LockoutEnd)
                                             .Ignore(c => c.ConcurrencyStamp);
                                             /*.Ignore(c => c.SecurityStamp)*/

            builder.Entity<ApplicationUser>().Property(x => x.UserName).HasMaxLength(32);
            builder.Entity<ApplicationUser>().Property(x => x.NormalizedUserName).HasMaxLength(32);
            builder.Entity<ApplicationUser>().Property(x => x.Email).HasMaxLength(64);
            builder.Entity<ApplicationUser>().Property(x => x.NormalizedEmail).HasMaxLength(64);
            builder.Entity<ApplicationUser>(entity => { entity.HasIndex(e => e.IdentificalNumber).IsUnique(); });
            builder.Entity<ApplicationUser>(entity => { entity.HasIndex(e => e.PhoneNumber).IsUnique(); });

            builder.Entity<IdentityRole>().Ignore(c => c.ConcurrencyStamp);

            builder.Entity<IdentityRole>().Property(x => x.Name).HasMaxLength(64);
            builder.Entity<IdentityRole>().Property(x => x.NormalizedName).HasMaxLength(64);

            builder.Entity<Loan>().Property(x => x.Paid).HasDefaultValue((decimal)0);
        }
    }
}

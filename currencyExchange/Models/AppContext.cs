using Microsoft.EntityFrameworkCore;

namespace currencyExchange.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity => {
                entity.HasKey(k => k.UserId);
            });

            modelBuilder.Entity<BankAccount>(entity => {
                entity.HasKey(k => k.AccountId);
            });

            modelBuilder.Entity<Transaction>(entity => {
                entity.HasKey(k => k.TransactionId);
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

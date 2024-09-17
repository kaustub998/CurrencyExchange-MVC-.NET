using Microsoft.EntityFrameworkCore;

namespace developerTask_CurrencyExchange.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}

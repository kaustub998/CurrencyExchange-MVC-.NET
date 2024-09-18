using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace currencyExchange.Models
{
    public class BankAccount
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? Currency { get; set; }
        public string? AccountType { get; set; }
        [Precision(18,2)]
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User? User { get; set; }
    }
}

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
        [Precision(18,2)]
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public string? BankAccountUserName { get; set; }
        public virtual User? User { get; set; }
    }

    public class SendMoneyModel
    {
        [Required]
        public int SourceAccountId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
        public string DestinationAccountName { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Account number must not exceed 20 characters.")]
        public string DestinationAccountNumber { get; set; }
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
    }

}

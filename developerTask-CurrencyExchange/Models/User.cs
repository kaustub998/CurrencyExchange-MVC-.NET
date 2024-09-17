using System.ComponentModel.DataAnnotations;

namespace developerTask_CurrencyExchange.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? PasswordHash { get; set; }
        public Guid SaltKey { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UserRegister : User
    {
        public string? Password { get; set; }
    }

    public class UserLogin 
    {
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BankSystem.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MinLength(3)]
        [MaxLength(64)]
        [Required]
        public string? Name { get; set; }

        [MinLength(3)]
        [MaxLength(64)]
        [Required]
        public string? LastName { get; set; }

        [MaxLength(512)]
        public string? Avatar { get; set; }

        [MaxLength(14)]
        [Required]
        public string? IdentificalNumber { get; set; }

        public List<BankAccount> BankAccounts { get; set; } = new();

        public List<Loan> Loans { get; set; } = new();
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem.Entities
{
    public class BankAccount
    {
        public int BankAccountID { get; set; }

        public string? UserID { get; set; }

        public ApplicationUser? User { get; set; }

        [Required]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Enter correct positive value (Max 2 decimal places)")]
        [Range(0, 60000.00, ErrorMessage = "Maximum sum is 60000.00")]
        public decimal Balance { get; set; }

        public string? NumberOfAccount { get; set; }

        [Required]
        public string Currency { get; set; }

        [InverseProperty("SenderAccount")]
        public List<Transaction>? SendTransactions { get; set; } = new();

        [InverseProperty("RecipientAccount")]
        public List<Transaction>? ReceivedTransactions { get; set; } = new();
    }
}

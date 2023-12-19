using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem.Entities
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        public int SenderID { get; set; }

        [ForeignKey("SenderID")]
        public BankAccount SenderAccount { get; set; }

        public int RecipientID { get; set; }

        [ForeignKey("RecipientID")]
        public BankAccount RecipientAccount { get; set; }

        [Required]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Enter correct positive value (Max 2 decimal places)")]
        [Precision(8, 2)]
        public decimal Sum { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
    }
}

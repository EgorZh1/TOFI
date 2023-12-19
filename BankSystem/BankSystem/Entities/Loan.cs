using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem.Entities
{
    public class Loan
    {
        public int LoanID { get; set; }

        public string? UserID { get; set; }

        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage="Deposit account filed is required")]
        public string? AccruedAccount { get; set; }

        [Required(ErrorMessage = "Payment account filed is required")]
        public string? PaymentAccount { get; set; }

        [Required(ErrorMessage = "Sum of loan filed is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Enter correct positive value (Max 2 decimal places)")]
        [Range(0, 300000.00, ErrorMessage = "Maximum sum is 300000.00")]
        public decimal SumOfLoan { get; set; }

        [Precision(8, 2)]
        public decimal? SumOfRefund { get; set; }
        
        [Precision(8, 2)]
        public decimal? Paid { get; set; }

        [Precision(8, 2)]
        public decimal? Remain { get; set; }

        [Precision(8, 2)]
        public decimal? Monthly { get; set; }

        [Required]
        public string Currency { get; set; }

        [Column(TypeName = "date")]
        public DateTime? BeginDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [Required]
        public int Months { get; set; }

        public string? Status { get; set; }
    }
}

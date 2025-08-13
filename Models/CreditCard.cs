using System.ComponentModel.DataAnnotations;

namespace CreditCardRegistration.Models
{
    public class CreditCard
    {
        public int CreditCardId { get; set; }

        [Required]
        [CreditCard(ErrorMessage = "Invalid card number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        public string CardType { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3-4 digits")]
        public string CVV { get; set; } = string.Empty;

        // Foreign key
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
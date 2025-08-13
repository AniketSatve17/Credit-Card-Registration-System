using System.ComponentModel.DataAnnotations;

namespace CreditCardRegistration.Models
{
    public class Option
    {
        public int OptionId { get; set; }

        [Required]
        public string OptionType { get; set; } = string.Empty;

        [Required]
        public string OptionValue { get; set; } = string.Empty;
    }
}
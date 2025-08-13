using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CreditCardRegistration.Models.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card number is required")]
        [CreditCard(ErrorMessage = "Invalid credit card number")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card type is required")]
        [Display(Name = "Card Type")]
        public string CardType { get; set; } = string.Empty;

        [ValidateNever] // This won't be automatically validated
        public FormControl? SpecialPromoControl { get; set; }

        [Required(ErrorMessage = "Identity document is required")]
        [DataType(DataType.Upload)]
        [Display(Name = "Identity Document")]
        public IFormFile IdentityDocument { get; set; } = null!;

        [ValidateNever] // Custom validation will be handled separately
        public string? AdminNotes { get; set; }
    }
}
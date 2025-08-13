using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreditCardRegistration.Models
{
    public class FormControl
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ControlId { get; set; }

        [Required(ErrorMessage = "Control type is required")]
        [StringLength(20, ErrorMessage = "Control type cannot exceed 20 characters")]
        [Display(Name = "Control Type")]
        public string ControlType { get; set; } = string.Empty; // Dropdown, Radio, Checkbox, Text

        [Required(ErrorMessage = "Control name is required")]
        [StringLength(50, ErrorMessage = "Control name cannot exceed 50 characters")]
        [Display(Name = "Control Name")]
        public string ControlName { get; set; } = string.Empty; // CountryList, GenderOptions, etc.

        [Required(ErrorMessage = "Option value is required")]
        [StringLength(100, ErrorMessage = "Option value cannot exceed 100 characters")]
        [Display(Name = "Option Value")]
        public string OptionValue { get; set; } = string.Empty; // USA, Male, Email Alerts, etc.

        [Display(Name = "Is Selected")]
        public bool IsSelected { get; set; } = false;

        [Display(Name = "Display Order")]
        [Range(0, 100, ErrorMessage = "Display order must be between 0 and 100")]
        public int DisplayOrder { get; set; } = 0;

        // Additional properties for enhanced functionality
        [StringLength(200)]
        [Display(Name = "Display Text")]
        public string? DisplayText { get; set; } // User-friendly display text

        [StringLength(50)]
        [Display(Name = "CSS Class")]
        public string? CssClass { get; set; } // For custom styling

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation property example (uncomment if needed)
        // public int? FormGroupId { get; set; }
        // public virtual FormGroup? FormGroup { get; set; }

        // Helper method
        [NotMapped]
        public string FullControlIdentifier => $"{ControlName}_{ControlId}";
    }
}
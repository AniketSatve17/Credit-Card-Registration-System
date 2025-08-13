using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CreditCardRegistration.Models; // Add this line

namespace CreditCardRegistration.Pages
{
    public class YourPageModel : PageModel
    {
        [ValidateNever]
        public FormControl? SpecialControl { get; set; }

        public void OnGet()
        {
            // Initialization code
        }
    }
}
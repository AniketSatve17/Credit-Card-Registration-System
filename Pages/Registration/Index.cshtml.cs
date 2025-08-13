using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CreditCardRegistration.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace CreditCardRegistration.Pages.Registration
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public RegistrationViewModel Registration { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            _logger.LogInformation("Registration page accessed at {Time}", DateTime.UtcNow);
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration submission. Errors: {Errors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return Page();
            }

            try
            {
                // Process registration logic here
                _logger.LogInformation("New registration received for {Email}", Registration.Email);

                // Store the registration data in TempData for the success page
                TempData["RegistrationData"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Registration.FirstName,
                    Registration.LastName,
                    Registration.Email,
                    Registration.CardType,
                    CardNumberLast4 = Registration.CardNumber?.Length > 4 ?
                        Registration.CardNumber.Substring(Registration.CardNumber.Length - 4) :
                        Registration.CardNumber,
                    SubmissionDate = DateTime.Now.ToString("f")
                });

                return RedirectToPage("/Registration/Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing registration for {Email}", Registration.Email);
                ErrorMessage = "An error occurred while processing your application. Please try again.";
                return Page();
            }
        }

        public JsonResult OnPostValidateField([FromBody] ValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.FieldName))
            {
                return new JsonResult(new { isValid = false, errors = new[] { "Field name is required" } });
            }

            var property = typeof(RegistrationViewModel).GetProperty(request.FieldName);
            if (property == null)
            {
                return new JsonResult(new { isValid = false, errors = new[] { "Invalid field name" } });
            }

            var context = new ValidationContext(Registration, null, null)
            {
                MemberName = request.FieldName
            };

            var results = new List<ValidationResult>();
            var value = property.GetValue(Registration);
            var isValid = Validator.TryValidateProperty(value, context, results);

            return new JsonResult(new
            {
                isValid,
                errors = results.Select(r => r.ErrorMessage)
            });
        }

        public class ValidationRequest
        {
            [Required]
            public string FieldName { get; set; } = null!;
        }
    }
}
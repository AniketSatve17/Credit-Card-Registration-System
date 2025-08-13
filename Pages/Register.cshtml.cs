using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CreditCardRegistration.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using System.Text.Json;
using CreditCardRegistration.Models;

namespace CreditCardRegistration.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public RegistrationInput Input { get; set; } = new();

        public SelectList CountryOptions { get; set; } = null!;
        public SelectList GenderOptions { get; set; } = null!;

        public RegisterModel(AppDbContext db, ILogger<RegisterModel> logger)
        {
            _db = db;
            _logger = logger;

            // Load dynamic options from database
            CountryOptions = new SelectList(_db.FormControls
                .Where(f => f.ControlName == "CountryList")
                .OrderBy(f => f.DisplayOrder),
                "OptionValue", "OptionValue");

            GenderOptions = new SelectList(_db.FormControls
                .Where(f => f.ControlName == "GenderOptions")
                .OrderBy(f => f.DisplayOrder),
                "OptionValue", "OptionValue");
        }

        public IActionResult OnGet()
        {
            try
            {
                // Initialize default values
                Input.Gender = "Unknown";
                Input.DateOfBirth = DateTime.Today.AddYears(-18);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading registration page");
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check for existing email
                if (await _db.Users.AnyAsync(u => u.Email == Input.Email))
                {
                    ModelState.AddModelError("Input.Email", "Email already registered");
                    return Page();
                }

                // Hash password before storing
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.Password);

                // Create and save user to database
                var user = new User
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    Password = hashedPassword,
                    PhoneNumber = Input.PhoneNumber,
                    DateOfBirth = Input.DateOfBirth,
                    Gender = Input.Gender,
                    Country = Input.Country,
                    CreatedDate = DateTime.UtcNow
                };

                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                // Update the Input with the generated UserId
                Input.UserId = user.Id;

                // Store in session for confirmation page
                HttpContext.Session.SetString("UserData",
                    JsonSerializer.Serialize(Input, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    }));

                // Call stored procedure if needed
                var parameters = new object[] {
                    user.Id,
                    Input.FirstName,
                    Input.LastName,
                    Input.Email,
                    hashedPassword,
                    Input.PhoneNumber ?? string.Empty,
                    Input.DateOfBirth,
                    Input.Gender,
                    Input.Country
                };

                await _db.Database.ExecuteSqlRawAsync(
                    "EXEC sp_RegisterUser {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                    parameters);

                return RedirectToPage("DocumentUpload");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error during registration");
                ModelState.AddModelError("", "A database error occurred during registration.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "An unexpected error occurred during registration.");
                return Page();
            }
        }

        public class RegistrationInput
        {
            [Required(ErrorMessage = "First name is required")]
            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = null!;

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = null!;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
            public string Email { get; set; } = null!;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
                ErrorMessage = "Password must contain uppercase, lowercase and number")]
            public string Password { get; set; } = null!;

            [Phone(ErrorMessage = "Invalid phone number format")]
            [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Date of birth is required")]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            [MinimumAge(18, ErrorMessage = "You must be at least 18 years old")]
            public DateTime DateOfBirth { get; set; }

            [Required(ErrorMessage = "Gender is required")]
            [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
            public string Gender { get; set; } = null!;

            [Required(ErrorMessage = "Country is required")]
            [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
            public string Country { get; set; } = null!;

            // Added UserId property for document upload
            public int UserId { get; set; }
        }
    }

    // Custom validation attribute for minimum age
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                if (dateOfBirth.AddYears(_minimumAge) > DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}
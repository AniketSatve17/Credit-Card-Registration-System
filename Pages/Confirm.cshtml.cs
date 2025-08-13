using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CreditCardRegistration.Data;
using CreditCardRegistration.Models;
using System.ComponentModel.DataAnnotations;

namespace CreditCardRegistration.Pages
{
    public class ConfirmModel : PageModel
    {
        private readonly ILogger<ConfirmModel> _logger;
        private readonly AppDbContext _dbContext;

        [BindProperty]
        public RegistrationData UserData { get; set; } = new();

        [BindProperty]
        public DocumentData DocumentInfo { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public ConfirmModel(ILogger<ConfirmModel> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var (userJson, docJson) = await LoadSessionDataAsync();
                if (!await ValidateSessionDataAsync(userJson, docJson))
                {
                    return RedirectToRegisterWithError("Session expired. Please start over.");
                }

                await DeserializeDataAsync(userJson!, docJson!);

                if (!await VerifyDatabaseStateAsync())
                {
                    return RedirectToRegisterWithError("Data validation failed. Please try again.");
                }

                _logger.LogInformation("Displaying confirmation for user {Email}", UserData.Email);
                return Page();
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, "loading confirmation data");
                return RedirectToRegisterWithError("An error occurred while loading your data.");
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            try
            {
                var (userJson, docJson) = await LoadSessionDataAsync();
                if (!await ValidateSessionDataAsync(userJson, docJson))
                {
                    return RedirectToRegisterWithError("Session expired. Please start over.");
                }

                await DeserializeDataAsync(userJson!, docJson!);

                if (!TryValidateModel(UserData))
                {
                    throw new ValidationException("Data validation failed");
                }

                await ProcessSubmissionAsync();

                _logger.LogInformation("Successful registration for {Email}", UserData.Email);
                StatusMessage = "Registration completed successfully!";
                return RedirectToPage("Success");
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, "processing registration");
                StatusMessage = $"Error completing registration: {ex.GetBaseException().Message}";
                return Page();
            }
        }

        private async Task<(string? userJson, string? docJson)> LoadSessionDataAsync()
        {
            return await Task.FromResult((
                HttpContext.Session.GetString("UserData"),
                HttpContext.Session.GetString("DocumentData")
            ));
        }

        private async Task<bool> ValidateSessionDataAsync(string? userJson, string? docJson)
        {
            if (string.IsNullOrEmpty(userJson) || string.IsNullOrEmpty(docJson))
            {
                _logger.LogWarning("Missing session data for confirmation");
                return false;
            }
            return await Task.FromResult(true);
        }

        private async Task DeserializeDataAsync(string userJson, string docJson)
        {
            UserData = JsonSerializer.Deserialize<RegistrationData>(userJson)
                ?? throw new InvalidOperationException("Invalid user data format");

            DocumentInfo = JsonSerializer.Deserialize<DocumentData>(docJson)
                ?? throw new InvalidOperationException("Invalid document data format");

            await Task.CompletedTask;
        }

        private async Task<bool> VerifyDatabaseStateAsync()
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == UserData.Email))
            {
                _logger.LogWarning("Duplicate registration attempt for {Email}", UserData.Email);
                return false;
            }
            return true;
        }

        private async Task ProcessSubmissionAsync()
        {
            var user = new User
            {
                FirstName = UserData.FirstName,
                LastName = UserData.LastName,
                Email = UserData.Email,
                Password = UserData.Password,
                PhoneNumber = UserData.PhoneNumber,
                DateOfBirth = UserData.DateOfBirth,
                Gender = UserData.Gender,
                Country = UserData.Country,
                CreatedDate = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            HttpContext.Session.Clear();
        }

        private IActionResult RedirectToRegisterWithError(string message)
        {
            StatusMessage = message;
            return RedirectToPage("Register");
        }

        private async Task LogErrorAsync(Exception ex, string operation)
        {
            _logger.LogError(ex, "Error {Operation} from {IP}",
                operation,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            await Task.CompletedTask;
        }

        public class RegistrationData
        {
            public int UserId { get; set; }

            [Required]
            public string FirstName { get; set; } = null!;

            [Required]
            public string LastName { get; set; } = null!;

            [Required, EmailAddress]
            public string Email { get; set; } = null!;

            [Required]
            public string Password { get; set; } = null!;

            [Phone]
            public string? PhoneNumber { get; set; }

            [Required]
            public DateTime DateOfBirth { get; set; }

            [Required]
            public string Gender { get; set; } = null!;

            [Required]
            public string Country { get; set; } = null!;
        }

        public class DocumentData
        {
            [Required]
            public string DocumentType { get; set; } = null!;

            [Required]
            public string DocumentName { get; set; } = null!;

            [Required]
            public string FileType { get; set; } = null!;

            public long FileSize { get; set; }
        }
    }
}
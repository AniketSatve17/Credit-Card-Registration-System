using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CreditCardRegistration.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CreditCardRegistration.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CreditCardRegistration.Pages
{
    public class DocumentUploadModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentUploadModel> _logger;

        [BindProperty]
        public DocumentUploadInput Input { get; set; } = new();

        public SelectList DocumentTypes { get; set; } = null!;

        public DocumentUploadModel(AppDbContext db, IWebHostEnvironment env, ILogger<DocumentUploadModel> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;

            DocumentTypes = new SelectList(_db.FormControls
                .Where(f => f.ControlName == "DocumentTypes")
                .OrderBy(f => f.DisplayOrder),
                "OptionValue", "OptionValue");
        }

        public IActionResult OnGet()
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserData")))
                {
                    return RedirectToPage("Register");
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading document upload page");
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
                // Validate session data
                var userDataJson = HttpContext.Session.GetString("UserData");
                if (string.IsNullOrEmpty(userDataJson))
                {
                    return RedirectToPage("Register");
                }

                // Deserialize with null check
                var userData = JsonSerializer.Deserialize<RegisterModel.RegistrationInput>(userDataJson);
                if (userData == null || Input?.DocumentFile == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid request data.");
                    return Page();
                }

                // Validate file
                if (Input.DocumentFile.Length == 0)
                {
                    ModelState.AddModelError("Input.DocumentFile", "The uploaded file is empty.");
                    return Page();
                }

                // File size limit (5MB)
                const long maxFileSize = 5 * 1024 * 1024;
                if (Input.DocumentFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("Input.DocumentFile", "The file size exceeds the 5MB limit.");
                    return Page();
                }

                // Validate file extensions
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(Input.DocumentFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Input.DocumentFile", "Only PDF, JPG, JPEG, and PNG files are allowed.");
                    return Page();
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.DocumentFile.CopyToAsync(stream);
                }

                // Save document info to database
                var document = new Document
                {
                    UserId = userData.UserId,
                    DocumentName = Input.DocumentFile.FileName,
                    DocumentType = Input.DocumentType,
                    DocumentPath = filePath,
                    FileSize = Input.DocumentFile.Length,
                    FileType = Input.DocumentFile.ContentType,
                    UploadDate = DateTime.UtcNow
                };

                await _db.Documents.AddAsync(document);
                await _db.SaveChangesAsync();

                // Clear session data after successful upload
                HttpContext.Session.Remove("UserData");

                return RedirectToPage("Confirm");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error during document upload");
                ModelState.AddModelError(string.Empty, "A database error occurred while saving your document.");
                return Page();
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "File I/O error during document upload");
                ModelState.AddModelError(string.Empty, "An error occurred while saving your file.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during document upload");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during document upload.");
                return Page();
            }
        }

        public class DocumentUploadInput
        {
            [Required(ErrorMessage = "Please select a document type")]
            [StringLength(50, ErrorMessage = "Document type cannot exceed 50 characters")]
            public string DocumentType { get; set; } = null!;

            [Required(ErrorMessage = "Please select a document to upload")]
            [DataType(DataType.Upload)]
            public IFormFile DocumentFile { get; set; } = null!;
        }
    }
}
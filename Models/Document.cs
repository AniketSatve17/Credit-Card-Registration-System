using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreditCardRegistration.Models
{
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Document name is required")]
        [StringLength(255, ErrorMessage = "Document name cannot exceed 255 characters")]
        public string DocumentName { get; set; } = null!;

        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot exceed 50 characters")]
        public string DocumentType { get; set; } = null!;

        [Required(ErrorMessage = "Document path is required")]
        [StringLength(500, ErrorMessage = "Document path cannot exceed 500 characters")]
        public string DocumentPath { get; set; } = null!;

        [Required(ErrorMessage = "File size is required")]
        [Range(0, long.MaxValue, ErrorMessage = "File size must be positive")]
        public long FileSize { get; set; }

        [Required(ErrorMessage = "File type is required")]
        [StringLength(100, ErrorMessage = "File type cannot exceed 100 characters")]
        public string FileType { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
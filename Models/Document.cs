using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grand_Arbre_portal.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string UploadedByUserId { get; set; } = string.Empty;

        public string UploadedByUserName { get; set; } = string.Empty;

        public string ClientUserId { get; set; } = string.Empty;

        public string ClientUserName { get; set; } = string.Empty;

        public string Direction { get; set; } = "EmployeeToClient"; 
        public string Status { get; set; } = "Active"; 

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public int DownloadCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [ForeignKey("UploadedByUserId")]
        public virtual ApplicationUser? UploadedByUser { get; set; }

        [ForeignKey("ClientUserId")]
        public virtual ApplicationUser? ClientUser { get; set; }
        public DateTime? ExpiryDate { get; set; }
        // Add this property
        public string Category { get; set; } = "Other";
    }
}
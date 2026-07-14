using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Grand_Arbre_portal.ViewModels
{
    public class ClientUploadViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Document Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Select File")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Document Category")]
        public string Category { get; set; } = "Other";

        [Display(Name = "Select Employee (Optional)")]
        public string SelectedEmployeeId { get; set; } = string.Empty;
    }
}
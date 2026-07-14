using System.ComponentModel.DataAnnotations;

namespace Grand_Arbre_portal.ViewModels
{
    public class CreateAnnouncementViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        public bool IsImportant { get; set; } = false;
    }
}
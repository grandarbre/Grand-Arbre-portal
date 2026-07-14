using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grand_Arbre_portal.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsImportant { get; set; } = false;

        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
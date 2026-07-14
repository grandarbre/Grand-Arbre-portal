using System.ComponentModel.DataAnnotations;

namespace Grand_Arbre_portal.Models
{
    public class AccessCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ForUserId { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;
        public string? UsedByUserId { get; set; }
        public DateTime? UsedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
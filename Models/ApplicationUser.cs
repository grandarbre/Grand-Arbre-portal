using Microsoft.AspNetCore.Identity;

namespace Grand_Arbre_portal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? AccessCode { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsFirstLogin { get; set; } = true;
        public string? ProfilePicturePath { get; set; } // Path to profile picture
    }
}
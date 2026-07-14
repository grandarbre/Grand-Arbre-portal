using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Grand_Arbre_portal.ViewModel
{
    public class ProfilePictureViewModel
    {
        [Required]
        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePicture { get; set; } = null!;
    }
}
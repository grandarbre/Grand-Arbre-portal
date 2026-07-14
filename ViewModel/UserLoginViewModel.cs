using System.ComponentModel.DataAnnotations;

namespace Grand_Arbre_portal.ViewModels
{
    public class UserLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string AccessCode { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
// ViewModels/CreateAccessCodeViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace GrandArbrePortal.ViewModels
{
    public class CreateAccessCodeViewModel
    {
        [Required]
        [Display(Name = "Access Code")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } // Employee or Client

        [Display(Name = "Expiry Date (Optional)")]
        public DateTime? ExpiryDate { get; set; }
    }
}
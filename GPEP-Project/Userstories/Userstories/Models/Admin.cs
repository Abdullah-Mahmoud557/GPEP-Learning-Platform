using System;
using System.ComponentModel.DataAnnotations;

namespace Userstories.Models
{
    public class Admin
    {
        public int AdminID { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[]? ImageData { get; set; }
    }
}

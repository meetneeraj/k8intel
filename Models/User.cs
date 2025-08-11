using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string ?PasswordHash { get; set; }
        [Required]
        public string? Role { get; set; } = "Admin";// "Admin", "Operator", "Viewer"
    }
}
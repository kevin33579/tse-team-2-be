using System.ComponentModel.DataAnnotations;

namespace UserApi.Models
{
    public class User
    {
        public int UserID { get; set; }
        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "")]
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public int RoleID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;







    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartApi.Models
{
    [Table("carts")]
    public class Cart
    {
        [Key]
        public int Id { get; set; }                 // maps to id INT PRIMARY KEY AUTO_INCREMENT

        public int UserId { get; set; }             // maps to user_id INT (FK)

        public DateTime CreatedAt { get; set; }     // maps to created_at DATETIME
                                                   // EF will respect DB default, but you can add:
                                                   // = DateTime.UtcNow;

        public bool IsCheckedOut { get; set; }      // maps to is_checked_out BOOLEAN
                                                   // default FALSE is handled by DB
        
        // ---- optional navigation properties ----
        // public User User { get; set; }            // if you have a User entity
        // public ICollection<CartItem> CartItems { get; set; }
        // public Order Order { get; set; }          // if one-to-one with Orders
    }
}

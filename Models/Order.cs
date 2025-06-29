using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tse_backend.Models
{
    public enum OrderStatus
    {
        PENDING,
        PAID,
        CANCELLED
    }

    [Table("orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }                  // id INT AUTO_INCREMENT PRIMARY KEY

        public int UserId { get; set; }              // user_id INT (FK → users.id)
        public int CartId { get; set; }              // cart_id INT (FK → carts.id)

        public DateTime OrderDate { get; set; }      // order_date DATETIME
                                                     // DB default handles value; optionally:
                                                     // = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }     // total_amount DECIMAL(10,2)

        public OrderStatus Status { get; set; }      // status ENUM('PENDING','PAID','CANCELLED')
                                                     // defaults to PENDING via DB

        /* ---- optional navigation properties ----
        public User User { get; set; }
        public Cart Cart { get; set; }
        */
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartItemApi.Models
{
    [Table("cart_items")]
    public class CartItem
    {
        [Key]
        public int Id { get; set; }          // id INT AUTO_INCREMENT PRIMARY KEY

        public int CartId { get; set; }      // cart_id INT NOT NULL   (FK → carts.id)
        public int ProductId { get; set; }   // product_id INT NOT NULL (FK → product.id)

        public int? ScheduleId { get; set; } // schedule_id INT NULL   (FK → schedule.id)

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }    // quantity INT

        /* ---- optional navigation properties ----
        public Cart     Cart     { get; set; }
        public Product  Product  { get; set; }
        public Schedule Schedule { get; set; }
        */
    }

    public class CartItemView
    {
        // cart_items columns
        public int  Id          { get; set; }
        public int  CartId      { get; set; }
        public int  ProductId   { get; set; }
        public int? ScheduleId  { get; set; }
        public int  Quantity    { get; set; }

        // carts columns (joined)
        public DateTime CreatedAt    { get; set; }
        public bool     IsCheckedOut { get; set; }
    }
}

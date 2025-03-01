using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class CartDetail
    {
        public int Id { get; set; }
        [Required]
        public int ShoppingCartId { get; set; }
       
        [Required]
        [ForeignKey("Dish")]
        public Guid DishId { get; set; }
     
        public virtual Dish Dish { get; set; }  // يجب أن تكون Virtual
        [Required]
        public int Quantity { get; set; }
        [ForeignKey("restaurant")]

        public Guid Restaurantid { get; set; }
        public virtual Restaurant restaurant { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")] // تحديد الدقة

       
        public decimal UnitPrice { get; set; }
        public virtual ShoppingCart ShoppingCart { get; set; }
    }
}

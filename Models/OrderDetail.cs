using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UsersApp.Models;

namespace UsersApp.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Dish")]
        public Guid DishId { get; set; }

        public virtual Dish Dish { get; set; }
        [ForeignKey("restaurant")]

        public Guid Restaurantid { get; set; }
        public virtual Restaurant restaurant { get; set; }// يجب أن تكون Virtual
        [Required]

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")] // تحديد الدقة
        public decimal UnitPrice { get; set; }
        public virtual Order Order { get; set; }
    }
}

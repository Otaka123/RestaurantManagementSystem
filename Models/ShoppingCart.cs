using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        //public bool IsGuestCart {  get; set; }=false;
        public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
    }
}

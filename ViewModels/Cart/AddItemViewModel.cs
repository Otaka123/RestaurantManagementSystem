using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels.Cart
{
    public class AddItemViewModel

    {
        public string? UserId { get; set; }
        public string? Guestid { get; set; }
        [Required]
        public Guid Dishid {  get; set; }

        [Range(1, 100)]
        public int Quantity {  get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels.Cart
{
    public class CheckoutViewModel
    {

        public string? UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}

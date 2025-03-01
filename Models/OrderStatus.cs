using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
      
        [MaxLength(20)]
        public string ?StatusName { get; set; }
    }
}

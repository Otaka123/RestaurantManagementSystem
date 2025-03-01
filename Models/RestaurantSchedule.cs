using System.ComponentModel.DataAnnotations.Schema;
using UsersApp.Models.Seed;

namespace UsersApp.Models
{
    public class RestaurantSchedule
    {
        public Guid Id { get; set; }

        [ForeignKey("Restaurant")]
        public Guid RestaurantId { get; set; }

        public int DayOfWeek { get; set; }

        public TimeOnly? OpeningTime { get; set; }

        public TimeOnly? ClosingTime { get; set; }
        public bool IsOpen { get; set; } = false; // إضافة خاصية IsOpen مع القيمة الافتراضية

        public virtual Restaurant Restaurant { get; set; } = null!;
    }
}

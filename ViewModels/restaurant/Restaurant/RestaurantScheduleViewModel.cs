using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels.Restaurant
{
    public class RestaurantScheduleViewModel
    {
        public Guid id = Guid.Empty;
        public Guid Restaurantid = Guid.Empty;
        public int DayOfWeek { get; set; }  // 0 = الأحد، 6 = السبت

        
        public TimeOnly? OpeningTime { get; set; }
        //[Required(ErrorMessage = "توقيت الإغلاق مطلوب")]
        //[RegularExpression(@"^([01]?[0-9]|2[0-3]):([0-5][0-9])$", ErrorMessage = "التنسيق غير صحيح. يجب أن يكون الوقت بتنسيق HH:mm.")]

        public TimeOnly?ClosingTime { get; set; }
        public bool IsOpen { get; set; } = false; // إضافة خاصية IsOpen مع القيمة الافتراضية

    }
}

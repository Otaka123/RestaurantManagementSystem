using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UsersApp.Data;
using UsersApp.Services;
using UsersApp.Validators;

namespace UsersApp.ViewModels
{
    public class RegisterViewModel
    {

        [Required(ErrorMessage = "الاسم الأول مطلوب.")]
        [RegularExpression(@"^[\u0621-\u064Aa-zA-Z]+$", ErrorMessage = "الاسم الاول يجب أن يحتوي على حروف فقط.")]

        [StringLength(50, ErrorMessage = "الاسم الأول لا يمكن أن يتجاوز 50 حرفاً.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الأخير مطلوب.")]
        [RegularExpression(@"^[\u0621-\u064Aa-zA-Z]+$", ErrorMessage = "اللقب يجب أن يحتوي على حروف فقط.")]

        [StringLength(50, ErrorMessage = "الاسم الأخير لا يمكن أن يتجاوز 50 حرفاً.")]
        public string LastName { get; set; } = string.Empty;

        ////[Required(ErrorMessage = "تاريخ الميلاد مطلوب.")]
        //[DataType(DataType.Date, ErrorMessage = "تنسيق التاريخ غير صالح.")]
        //[CustomValidation(typeof(Validation), nameof(Validation.ValidateDOB))]
        public DateTime? DOB { get; set; }


        [Required(ErrorMessage = "وقت الإنشاء مطلوب.")]
        public DateTime Createtime { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "اسم المستخدم يجب أن يكون بين 5 و 30 حرفاً.")]
        public string UserName { get; set; }
        [Phone]
        [RegularExpression(@"^[\u0660-\u0669\d]+$", ErrorMessage = "الحقل يجب أن يحتوي على أرقام فقط.")]
        [Required(ErrorMessage = "رقم الموبيل مطلوب")]
        [UniquePhone(ErrorMessage = "رقم الهاتف موجود بالفعل.")]
        public string Phone { get; set; }


        [Required(ErrorMessage = "الايميل مطلوب")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    
        [Required(ErrorMessage = "تاكيد الباسورد مطلوب.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm  Password")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور لا تتطابقان")]
        public string ConfirmPassword { get; set; }
        //[Required(ErrorMessage = "النوع الأول مطلوب.")]
        [RegularExpression(@"^[\u0621-\u064Aa-zA-Z]+$", ErrorMessage = "الاسم الاول يجب أن يحتوي على حروف فقط.")]

        public string? Gender {  get; set; }
        [Required(ErrorMessage = "Please select an option.")]
        public string UserType { get; set; }
    }
}

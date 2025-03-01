using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "يجب أن يكون اسم المستخدم بين 3 و 50 حرفًا.")]
       public string Login { get; set; }

        //[Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        //[EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح.")]
        //public string Email { get; set; }

        [Required(ErrorMessage = "كلمه السر مطلوبه")]
        [DataType(DataType.Password)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        //    ErrorMessage = "يجب أن تحتوي كلمة المرور على حرف كبير، حرف صغير، رقم، ورمز خاص واحد على الأقل.")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

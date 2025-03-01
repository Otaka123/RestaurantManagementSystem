using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UsersApp.Data;
using UsersApp.Services.Account;

namespace UsersApp.Validators
{

    public class UniquePhoneAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // لا حاجة للتحقق إذا كانت القيمة فارغة
            }

            // استدعاء الخدمة من خلال الـ ValidationContext
            var accountService = (IAccountService)validationContext.GetService(typeof(IAccountService));
            if (accountService == null)
            {
                throw new InvalidOperationException("تعذر العثور على الخدمة IAccountService.");
            }

            // التحقق من رقم الهاتف
            var phoneNumber = value.ToString();
            var user = accountService.GetUserByPhoneAsync(phoneNumber).Result; // استخدم .Result لأنها Async
            if (user != null)
            {
                return new ValidationResult("رقم الهاتف موجود بالفعل.");
            }

            return ValidationResult.Success;
        }

    }
}


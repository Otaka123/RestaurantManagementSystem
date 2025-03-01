using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using UsersApp.Data;
using UsersApp.Models;
using UsersApp.Services;

namespace UsersApp.Validators
{
    public static class Validation
    {

        public static ValidationResult ValidateDOB(DateTime dob)
        {
            if (dob > DateTime.UtcNow)
            {
                return new ValidationResult("تاريخ الميلاد لا يمكن أن يكون في المستقبل.");
            }

            if ((DateTime.UtcNow.Year - dob.Year) < 18)
            {
                return new ValidationResult("يجب أن يكون عمر المستخدم 18 سنة أو أكثر.");
            }

            return ValidationResult.Success;
        }
       
    }

}

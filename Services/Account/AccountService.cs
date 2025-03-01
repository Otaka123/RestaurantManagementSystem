using IdentityManager.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using UsersApp.Common.Results;
using UsersApp.Controllers;
using UsersApp.Models;
using UsersApp.Services.Email;
using UsersApp.ViewModels;
using static UsersApp.Services.Account.AccountService;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace UsersApp.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountController> _logger;


        public AccountService(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IUrlHelperFactory urlHelperFactory,
            IHttpContextAccessor httpContextAccessor,
            SignInManager<Users> signInManager , IWebHostEnvironment hostEnvironment, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }
        public async Task<Users> GetUserAsync(ClaimsPrincipal User)
        {
         return await   _userManager.GetUserAsync(User);
        }
        public async Task<IList<string>> GetRolesAsync(Users user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<IdentityResult> RemoveFromRoleAsync(Users user,string roleName)
        {
            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }
        public async Task<Users>FindByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
        public async Task<IdentityResult> AddToRoleAsync(Users user,string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }
        public async Task<SignInResult> LoginAsync(string login, string password, bool rememberMe)
        {
            var existingUser = await _userManager.Users
                       .FirstOrDefaultAsync(u =>
                           u.Email == login || u.UserName == login || u.PhoneNumber == login);

            if (existingUser == null)
            {
                return SignInResult.Failed;
            }

            // التحقق من صحة بيانات تسجيل الدخول
            var result = await _signInManager.PasswordSignInAsync(existingUser, password, rememberMe, lockoutOnFailure: true);
            return result;
        }

        public async Task<Users> GetUserByPhoneAsync(string phone)
        {
            return _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phone);// يعيد كل المستخدمين
        }
        public async Task<Result<RegisterViewModel>> RegisterAsync(RegisterViewModel model, string returnUrl)
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == model.Email))
            {
                string message = "البريد الإلكتروني مسجل بالفعل.";
                _logger.LogWarning("فشل إنشاء المستخدم: {Message} - Email: {Email}", message, model.Email);
                return Result<RegisterViewModel>.Failure(message);
            }

            // التحقق من وجود المستخدم بنفس رقم الهاتف
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.Phone))
            {
                string message = "رقم الهاتف مسجل بالفعل.";
                _logger.LogWarning("فشل إنشاء المستخدم: {Message} - PhoneNumber: {PhoneNumber}", message, model.Phone);
                return Result<RegisterViewModel>.Failure(message);
            }

            var user = new Users
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DOB = model.DOB,
                Createtime = DateTime.UtcNow,
                PhoneNumber = model.Phone,
                Gender = model.Gender,

            };
            try
            {
                if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    user.ProfilePictureUrl = GetDefaultProfilePictureUrl(user.Gender);

                }
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var role = await _roleManager.FindByNameAsync(model.UserType);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }

                    // إنشاء IUrlHelper باستخدام IUrlHelperFactory
                    var urlHelper = _urlHelperFactory.GetUrlHelper(new ActionContext
                    {
                        HttpContext = _httpContextAccessor.HttpContext,
                        RouteData = _httpContextAccessor.HttpContext.GetRouteData(),
                        ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
                    });

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = urlHelper.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userid = user.Id, code },
                        protocol: "https");

                    await _emailService.SendEmailAsync(
                        model.Email,
                        "Confirm Email - Identity Manager",
                        $"Please confirm your email by clicking here: <a href=' {callbackUrl} '> press here </a>"
                    );
                    string message = "تم انشاء الحساب بنجاح";
                    _logger.LogInformation(message, model.Email);
                    return Result<RegisterViewModel>.CreateSuccess(model,message);
                }
                else
                {
                    string massage = "خطأ أثناء إنشاء المستخدم - Email: {Email}";
                    _logger.LogError( massage, model.Email);
                    return Result<RegisterViewModel>.Failure(massage);
                }
            }
            catch(Exception ex)
            {
                string massage = "خطأ أثناء إنشاء المستخدم - Email: {Email}";
                _logger.LogError(ex,massage, model.Email);
                return Result<RegisterViewModel>.Failure(massage);

            }
        }
        public async Task<string> GetReturnUrlAsync(string returnUrl)
        {
            // إعادة معالجة returnUrl إذا لزم الأمر (يمكنك إضافة أي منطق هنا)
            return returnUrl ?? "/";
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }
        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }
        public async Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo info)
        {
            return await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

        }
        public async Task<Users> FindByEmailAsync(string Email)
        {
            return await _userManager.FindByEmailAsync(Email);
        }
        public async Task<SignInResult> ExternalLoginSignInAsync(string provider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            return await _signInManager.ExternalLoginSignInAsync(provider, providerKey, isPersistent: false, bypassTwoFactor: true);
        }

        public async Task<SignInResult> CreateUserAndLoginAsync(ExternalLoginConfirmationViewModel model, ExternalLoginInfo info)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // إنشاء مستخدم جديد
                user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Createtime = DateTime.UtcNow,
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                        return SignInResult.Success;
                    }
                }
                return SignInResult.Failed;
            }

            // إذا كان المستخدم موجودًا
            await _signInManager.SignInAsync(user, isPersistent: false);
            await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
            return SignInResult.Success;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User not found. UserId: {UserId}", userId);

            }

            return await _userManager.ConfirmEmailAsync(user, code);
        }
        public async Task<string> GeneratePasswordResetTokenAsync(Users user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(Users user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task SendResetPasswordEmailAsync(string email, string resetLink)
        {
            await _emailService.SendEmailAsync(email, "Reset Password",
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>");
        }

        //public async Task<IdentityResult> CreateUserAndLoginAsync(ExternalLoginConfirmationViewModel model, ExternalLoginInfo info)
        // {
        //     var user = new Users
        //     {
        //         UserName = model.Email,
        //         Email = model.Email,
        //         Createtime = DateTime.UtcNow
        //     };

        //     var result = await _userManager.CreateAsync(user);
        //     if (result.Succeeded)
        //     {
        //         result = await _userManager.AddLoginAsync(user, info);
        //         if (result.Succeeded)
        //         {
        //             await _signInManager.SignInAsync(user, isPersistent: false);
        //             await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
        //         }
        //     }

        //     return result;

        // }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        public  List<Users> GetAllUsers()
        {
            return  _userManager.Users.ToList();
        }
        public async Task<List<Users>> GetAllUsersAsync()
        {
          return  await  _userManager.Users.ToListAsync();
        }
        public async Task<IdentityResult> UpdateAsync(Users user)
        {
            return await _userManager.UpdateAsync(user);
        }


        public async Task<IdentityResult> UpdateUserProfileAsync(EditProfileViewModel model, ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                _logger.LogError("User not found. UserId: {UserId}", userPrincipal.Identity.Name);
            }

            // معالجة الصورة المرفوعة
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfilePicture.FileName);
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "profilePictures");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = $"/images/profilePictures/{fileName}";
            }
            else
            {
                // تحديث الصورة الافتراضية بناءً على الجنس
                user.ProfilePictureUrl = model.Gender == "Male"
                    ? "/images/defaults/default_male.png"
                    : model.Gender == "Female"
                        ? "/images/defaults/default_female.png"
                        : "/images/defaults/default_unknown.png";
            }

            // تحديث البيانات الأخرى
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Gender = model.Gender;

            return await _userManager.UpdateAsync(user);
        }


        //public async Task<IdentityResult> UpdateUserProfileAsync(EditProfileViewModel model, ClaimsPrincipal userPrincipal)
        //{
        //    var user = await _userManager.GetUserAsync(userPrincipal);
        //    if (user == null)
        //    {
        //        _logger.LogError("User not found. UserId: {UserId}", userPrincipal.Identity.Name);
        //        return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        //    }

        //    // معالجة الصورة المرفوعة
        //    if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        //    {
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        //        var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

        //        if (!allowedExtensions.Contains(extension))
        //        {
        //            _logger.LogWarning("Invalid file type uploaded by user {UserId}", user.Id);
        //            return IdentityResult.Failed(new IdentityError { Description = "Only image files are allowed." });
        //        }

        //        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        //        if (model.ProfilePicture.Length > maxFileSize)
        //        {
        //            _logger.LogWarning("File size exceeds limit for user {UserId}", user.Id);
        //            return IdentityResult.Failed(new IdentityError { Description = "The file size should not exceed 5 MB." });
        //        }

        //        // تحقق من أن الملف صورة حقيقية
        //        using (var stream = model.ProfilePicture.OpenReadStream())
        //        {
        //            try
        //            {
        //                var image = System.Drawing.Image.FromStream(stream);
        //            }
        //            catch (Exception)
        //            {
        //                _logger.LogWarning("Invalid image file uploaded by user {UserId}", user.Id);
        //                return IdentityResult.Failed(new IdentityError { Description = "Invalid image file." });
        //            }
        //        }

        //        // حذف الصورة القديمة إن وجدت
        //        if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && !user.ProfilePictureUrl.Contains("defaults"))
        //        {
        //            var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
        //            if (System.IO.File.Exists(oldFilePath))
        //            {
        //                System.IO.File.Delete(oldFilePath);
        //            }
        //        }

        //        var fileName = Guid.NewGuid() + extension;
        //        var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "profilePictures");
        //        Directory.CreateDirectory(uploadsFolder);

        //        var filePath = Path.Combine(uploadsFolder, fileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await model.ProfilePicture.CopyToAsync(stream);
        //        }

        //        user.ProfilePictureUrl = $"/images/profilePictures/{fileName}";
        //    }
        //    else
        //    {
        //        // تحديث الصورة الافتراضية بناءً على الجنس
        //        user.ProfilePictureUrl = model.Gender == "Male"
        //            ? "/images/defaults/default_male.png"
        //            : model.Gender == "Female"
        //                ? "/images/defaults/default_female.png"
        //                : "/images/defaults/default_unknown.png";
        //    }

        //    // تحديث البيانات الأخرى
        //    user.UserName = model.UserName;
        //    user.Email = model.Email;
        //    user.Gender = model.Gender;

        //    return await _userManager.UpdateAsync(user);
        //}


        //public async Task<string> UploadProfilePictureAsync(IFormFile profilePicture, string existingImagePath = null)
        //{
        //    if (profilePicture == null || profilePicture.Length == 0)
        //    {
        //        throw new ArgumentException("No file provided.");
        //    }

        //    // التحقق من نوع الصورة باستخدام ContentType
        //    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        //    if (!allowedMimeTypes.Contains(profilePicture.ContentType.ToLower()))
        //    {
        //        _logger.LogWarning("Invalid MIME type: {MimeType}", profilePicture.ContentType);
        //        throw new InvalidOperationException("Only JPEG, PNG, and GIF images are allowed.");
        //    }

        //    // التحقق من حجم الصورة
        //    const long maxFileSize = 5 * 1024 * 1024; // 5MB
        //    if (profilePicture.Length > maxFileSize)
        //    {
        //        _logger.LogWarning("File size exceeds limit: {FileSize}", profilePicture.Length);
        //        throw new InvalidOperationException("The file size should not exceed 5 MB.");
        //    }

        //    // حذف الصورة القديمة إن وجدت
        //    if (!string.IsNullOrEmpty(existingImagePath) && !existingImagePath.Contains("defaults"))
        //    {
        //        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingImagePath.TrimStart('/'));
        //        if (File.Exists(oldFilePath))
        //        {
        //            File.Delete(oldFilePath);
        //        }
        //    }
        //    var fileName = Guid.NewGuid() + Path.GetExtension(profilePicture.FileName);

        //    var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "profilePictures");
        //    Directory.CreateDirectory(uploadsFolder);


        //    // إنشاء اسم فريد للصورة الجديدة
        //    var filePath = Path.Combine(uploadsFolder, fileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await profilePicture.CopyToAsync(stream);
        //    }

        //    _logger.LogInformation("Profile picture uploaded successfully: {FileName}", fileName);
        //    return $"/images/profilePictures/{fileName}";
        //}
        public async Task<Result<string>> UploadPictureAsync(IFormFile profilePicture, string existingImagePath = null)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                _logger.LogWarning("No file provided.");
                return Result<string>.Failure("No file provided.");
            }

            // التحقق من نوع الصورة باستخدام ContentType
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedMimeTypes.Contains(profilePicture.ContentType.ToLower()))
            {
                _logger.LogWarning("Invalid MIME type: {MimeType}", profilePicture.ContentType);
                return Result<string>.Failure("Only JPEG, PNG, and GIF images are allowed.");
            }

            // التحقق من حجم الصورة
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (profilePicture.Length > maxFileSize)
            {
                _logger.LogWarning("File size exceeds limit: {FileSize}", profilePicture.Length);
                return Result<string>.Failure("The file size should not exceed 5 MB.");
            }

            try
            {
                // حذف الصورة القديمة إن وجدت
                if (!string.IsNullOrEmpty(existingImagePath) && !existingImagePath.Contains("defaults"))
                {
                    var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingImagePath.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // إنشاء مجلد الصور إن لم يكن موجودًا
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "profilePictures");
                Directory.CreateDirectory(uploadsFolder);

                // إنشاء اسم فريد للصورة الجديدة
                var fileName = Guid.NewGuid() + Path.GetExtension(profilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                _logger.LogInformation("Profile picture uploaded successfully: {FileName}", fileName);
                return Result<string>.CreateSuccess($"/images/profilePictures/{fileName}", "Profile picture uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading the profile picture.");
                return Result<string>.Failure("An unexpected error occurred while uploading the profile picture.");
            }
        }

        public string GetDefaultProfilePictureUrl(string gender)
        {
            if (gender == "Male")
            {
                return "/images/defaults/default_male.png";
            }
            else if (gender == "Female")
            {
                return "/images/defaults/default_female.png";
            }
            else
            {
                return "/images/defaults/default_unknown.png";
            }
        }

      
    }

}


using IdentityManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Text.Encodings.Web;
using UsersApp.Common.Message;
using UsersApp.Models;
using UsersApp.Services.Account;
using UsersApp.Services.Email;
using UsersApp.Validators;
using UsersApp.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UsersApp.Controllers
{
    [Authorize]

    public class AccountController : Controller
    {
        //private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly SignInManager<Users> _signInManager;
        //private readonly IEmailService _emailService;
        //private readonly UrlEncoder _urlEncoder;
        //private readonly UserManager<Users> _userManager;
        private readonly IAccountService _accountService;
        //private readonly IWebHostEnvironment _hostEnvironment;

        public AccountController(IAccountService accountService)
        {
            //_emailService = emailSender;
            //_urlEncoder = urlEncoder;
            //_signInManager = signInManager;
            //_userManager = userManager;
            //_roleManager = roleManager;
            _accountService = accountService;
        }
        [AllowAnonymous]
        public IActionResult Getstarted(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Getstarted()
        {
            return View();

        }
        [AllowAnonymous]
        public IActionResult Register(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }
        private void SetTempMessage(string message, MessageType type)
        {
            TempData[type == MessageType.Success ? "SuccessMessage" : "ErrorMessage"] = message;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = GetReturnUrl(returnUrl);

            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterAsync(model, returnUrl);

                if (result.Success)
                {
                    SetTempMessage(result.Message, MessageType.Success);

                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    SetTempMessage(result.Message, MessageType.Error);
                    return View(model);

                }


            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }


       
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            // إعداد returnUrl
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = await _accountService.GetReturnUrlAsync(returnUrl);

            if (ModelState.IsValid)
            {
                // استدعاء الخدمة لتسجيل الدخول
                var result = await _accountService.LoginAsync(model.Login, model.Password, model.RememberMe);

                if (result.Succeeded)
                {
                    // إعادة التوجيه بعد تسجيل الدخول بنجاح
                    if(User.IsInRole("Owner"))
                    {
                        return RedirectToAction("Index", "RestaurantDashboard");
                    }
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Email or password is incorrect.");
                    return View(model);
                }
            }
            return View(model);
        }
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
        //{
        //    returnurl = GetReturnUrl(returnurl);
        //    if (remoteError != null)
        //    {
        //        ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
        //        return View(nameof(Login));
        //    }

        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return RedirectToAction(nameof(Login));
        //    }

        //    //sign in the user with this external login provider. only if they have a login
        //    var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
        //                       isPersistent: false, bypassTwoFactor: true);
        //    if (result.Succeeded)
        //    {
        //        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
        //        return LocalRedirect(returnurl);
        //    }


        //        //that means user account is not create and we will display a view to create an account

        //        ViewData["ReturnUrl"] = returnurl;
        //        ViewData["ProviderDisplayName"] = info.ProviderDisplayName;

        //        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
        //        {
        //            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
        //            Name = info.Principal.FindFirstValue(ClaimTypes.Name)
        //        });


        //}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
        {
            returnurl = GetReturnUrl(returnurl);
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _accountService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _accountService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            if (result.Succeeded)
            {
                await _accountService.UpdateExternalAuthenticationTokensAsync(info);
                return LocalRedirect(returnurl);
            }

            ViewData["ReturnUrl"] = returnurl;
            ViewData["ProviderDisplayName"] = info.ProviderDisplayName;

            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
            {
                Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                Name = info.Principal.FindFirstValue(ClaimTypes.Name)
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnurl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnurl });
            var properties = _accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            returnUrl = GetReturnUrl(returnUrl);

            if (ModelState.IsValid)
            {
                var info = await _accountService.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("Error");
                }

                var result = await _accountService.CreateUserAndLoginAsync(model, info);
                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "External login failed.");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }
        private string GetReturnUrl(string returnurl) => returnurl ?? Url.Content("~/");

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string code, string userId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _accountService.ConfirmEmailAsync(userId, code);
                    if (result.Succeeded)
                    {
                        return View();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email confirmation failed.");
                    }
                }
                catch (ArgumentException)
                {
                    return View("Error");
                }
            }
            return View("Error");
        }
        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _accountService.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // لا تفصح عن وجود المستخدم لأسباب أمان
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                // إنشاء رمز إعادة تعيين كلمة المرور
                var token = await _accountService.GeneratePasswordResetTokenAsync(user);

                // إنشاء رابط إعادة التعيين
                var resetLink = Url.Action("ResetPassword", "Account", new
                {
                    email = model.Email,
                    token,
                }, Request.Scheme);

                // إرسال البريد الإلكتروني
                await _accountService.SendResetPasswordEmailAsync(model.Email, resetLink);

                return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _accountService.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // لا تفصح عن وجود المستخدم لأسباب أمان
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _accountService.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ChangePasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

      
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
      

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email= username });
        }
        //[HttpPost]
        //public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByNameAsync(model.Email);

        //        if(user == null)
        //        {
        //            ModelState.AddModelError("", "Something is wrong!");
        //            return View(model);
        //        }
        //        else
        //        {
        //            return RedirectToAction("ChangePassword","Account", new {username = user.UserName});
        //        }
        //    }
        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByNameAsync(model.Email);
        //        if(user != null)
        //        {
        //            var result = await _userManager.RemovePasswordAsync(user);
        //            if (result.Succeeded)
        //            {
        //                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        //                return RedirectToAction("Login", "Account");
        //            }
        //            else
        //            {

        //                foreach (var error in result.Errors)
        //                {
        //                    ModelState.AddModelError("", error.Description);
        //                }

        //                return View(model);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Email not found!");
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("", "Something went wrong. try again.");
        //        return View(model);
        //    }
        //}
        //[AllowAnonymous]
        //public IActionResult Profile(string returnurl = null)
        //{
        //    ViewData["ReturnUrl"] = returnurl;
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> UpdateProfilePicture(ProfilePictureViewModel model)
        //{
        //    if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        //    {
        //        var user = await _accountService.GetUserAsync(User);

        //        if (user != null)
        //        {
        //            // استخدام الخدمة لحفظ الصورة
        //            var profilePictureUrl = await _accountService.SaveProfilePictureAsync(model.ProfilePicture, user.Id,user.Gender);

        //            if (!string.IsNullOrEmpty(profilePictureUrl))
        //            {
        //                user.ProfilePictureUrl = profilePictureUrl;
        //                await _accountService.UpdateAsync(user);
        //            }
        //        }
        //    }

        //    return RedirectToAction("Profile", "Account");
        //}
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _accountService.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Gender = user.Gender,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _accountService.GetUserAsync(User);
        //        if (user == null)
        //        {
        //            return NotFound("User not found");
        //        }

        //        // معالجة الصورة المرفوعة
        //        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        //        {
        //            var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfilePicture.FileName);
        //            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profilePictures", fileName);

        //            // إنشاء المجلد إذا لم يكن موجودًا
        //            var directory = Path.GetDirectoryName(filePath);
        //            if (!Directory.Exists(directory))
        //            {
        //                Directory.CreateDirectory(directory);
        //            }

        //            // حفظ الملف
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await model.ProfilePicture.CopyToAsync(stream);
        //            }

        //            // تحديث رابط الصورة
        //            user.ProfilePictureUrl = $"/images/profilePictures/{fileName}";
        //        }

        //        // إذا لم يتم رفع صورة، احتفظ بالرابط الحالي أو استخدم الصورة الافتراضية
        //        user.ProfilePictureUrl ??= "/images/defaults/default_unknown.png";

        //        // تحديث البيانات الأخرى
        //        user.UserName = model.UserName;
        //        user.Email = model.Email;
        //        user.Gender = model.Gender;

        //        var result = await _accountService.UpdateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }

        //        // إضافة الأخطاء إذا فشل التحديث
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error.Description);
        //        }
        //    }

        //    return View(model);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _accountService.UpdateUserProfileAsync(model, User);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}

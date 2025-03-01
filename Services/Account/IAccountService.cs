using IdentityManager.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UsersApp.Common.Results;
using UsersApp.Models;
using UsersApp.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace UsersApp.Services.Account
{
    public interface IAccountService
    {
        Task<IdentityResult> UpdateAsync(Users user);
        Task<IdentityResult> UpdateUserProfileAsync(EditProfileViewModel model, ClaimsPrincipal userPrincipal);
        Task<Users> GetUserAsync(ClaimsPrincipal User);
        List<Users> GetAllUsers();
        Task<List<Users>> GetAllUsersAsync();
        string GetDefaultProfilePictureUrl(string gender);
        Task<IList<string>> GetRolesAsync(Users user);
        Task<IdentityResult> RemoveFromRoleAsync(Users user, string roleName);
        Task<IdentityResult> AddToRoleAsync(Users user, string roleName);
        Task<Users> FindByIdAsync(string userId);
        Task<Users> GetUserByPhoneAsync(string phone);
        Task<Users> FindByEmailAsync(string Email);
        Task<Result<RegisterViewModel>> RegisterAsync(RegisterViewModel model, string returnUrl);
        Task<SignInResult> LoginAsync(string login, string password, bool rememberMe);
        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string provider, string providerKey, bool isPersistent, bool bypassTwoFactor);
        Task<SignInResult> CreateUserAndLoginAsync(ExternalLoginConfirmationViewModel model, ExternalLoginInfo info);
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string code);
        Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo info);
        Task<string> GetReturnUrlAsync(string returnUrl);
        Task<string> GeneratePasswordResetTokenAsync(Users user);
        Task<IdentityResult> ResetPasswordAsync(Users user, string token, string newPassword);
        Task SendResetPasswordEmailAsync(string email, string resetLink);
        Task SignOutAsync();
        Task<Result<string>> UploadPictureAsync(IFormFile profilePicture, string existingImagePath = null);
    }
}

using Microsoft.AspNetCore.Identity;

namespace UsersApp.Services.Role
{
    public interface IRoleService
    {
        Task<List<IdentityRole>> GetAllRolesAsync();
        Task<IdentityRole> GetRoleByIdAsync(string roleId);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName);
        Task<IdentityResult> DeleteRoleAsync(string roleId);
        Task<IdentityResult> AddRoleToUserAsync(string userId, string roleName);
        Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(string userId);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsersApp.Services.Account;

namespace UsersApp.Services.Role
{
    public class RoleService: IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccountService _accountserves;
        public RoleService(RoleManager<IdentityRole> roleManager, IAccountService accountserves)
        {
            _roleManager = roleManager;
            _accountserves= accountserves;
        }

        public async Task<List<IdentityRole>> GetAllRolesAsync()
        {
            return new List<IdentityRole>(await _roleManager.Roles.ToListAsync());
        }

        public async Task<IdentityRole> GetRoleByIdAsync(string roleId)
        {
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            var role = new IdentityRole(roleName);
            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                role.Name = newRoleName;
                return await _roleManager.UpdateAsync(role);
            }
            return IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                return await _roleManager.DeleteAsync(role);
            }
            return IdentityResult.Failed();
        }
        //public async Task<bool> AddRoleToUserAsync(string userId, string roleName)
        //{
        //    var user = await _accountserves.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new ArgumentException("User not found");
        //    }

        //    if (!await _roleManager.RoleExistsAsync(roleName))
        //    {
        //        throw new ArgumentException("Role does not exist");
        //    }

        //    var result = await _accountserves.AddToRoleAsync(user, roleName);
        //    return result.Succeeded;
        //}

        //public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        //{
        //    var user = await _accountserves.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new ArgumentException("User not found");
        //    }

        //    if (!await _roleManager.RoleExistsAsync(roleName))
        //    {
        //        throw new ArgumentException("Role does not exist");
        //    }

        //    var result = await _accountserves.RemoveFromRoleAsync(user, roleName);
        //    return result.Succeeded;
        //}

        //public async Task<IList<string>> GetUserRolesAsync(string userId)
        //{
        //    var user = await _accountserves.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new ArgumentException("User not found");
        //    }

        //    return await _accountserves.GetRolesAsync(user);
        //}
        // إضافة دور إلى مستخدم
        public async Task<IdentityResult> AddRoleToUserAsync(string userId, string roleName)
        {
            var user = await _accountserves.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(userId));

            if (!await _roleManager.RoleExistsAsync(roleName))
                return IdentityResult.Failed(new IdentityError { Description = "Role does not exist." });

            return await _accountserves.AddToRoleAsync(user, roleName);
        }

        // إزالة دور من مستخدم
        public async Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _accountserves.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(userId));

            if (!await _roleManager.RoleExistsAsync(roleName))
                return IdentityResult.Failed(new IdentityError { Description = "Role does not exist." });

            return await _accountserves.RemoveFromRoleAsync(user, roleName);
        }

        // عرض أدوار مستخدم معين
        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _accountserves.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(userId));

            return new List<string>(await _accountserves.GetRolesAsync(user));
        }
    }
}


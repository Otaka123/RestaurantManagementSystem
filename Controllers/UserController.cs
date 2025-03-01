using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsersApp.Services.Account;
using UsersApp.Services.Role;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IAccountService _AccountService;
        private readonly IRoleService _roleManager;

        public UserController(IAccountService AccountService, IRoleService roleManager)
        {
            _AccountService = AccountService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> UsersWithRoles(string searchTerm)
        {
            var users = _AccountService.GetAllUsers();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _AccountService.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = string.Join(", ", roles)
                });
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model = model
                    .Where(u => u.UserName.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                                u.Roles.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(model);
        }

        
     public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await _AccountService.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("sadasd");
            }

            var userRoles = await _AccountService.GetRolesAsync(user);
            var roles = await _roleManager.GetAllRolesAsync();

            ViewBag.UserId = userId;
            ViewBag.UserName = user.UserName;

            var model = roles.Select(role => new ManageroleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                IsSelected = userRoles.Contains(role.Name)
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(string userId, List<ManageroleViewModel> model)
        {
            var user = await _AccountService.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _AccountService.GetRolesAsync(user);

            foreach (var role in model)
            {
                if (role.IsSelected && !userRoles.Contains(role.RoleName))
                {
                    await _AccountService.AddToRoleAsync(user, role.RoleName);
                }
                else if (!role.IsSelected && userRoles.Contains(role.RoleName))
                {
                    await _AccountService.RemoveFromRoleAsync(user, role.RoleName);
                }
            }

            return RedirectToAction("UsersWithRoles");
        }
       
    }
}


using Microsoft.AspNetCore.Mvc;
using UsersApp.Services.Role;

namespace UsersApp.Controllers
{
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // عرض جميع الأدوار
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        // عرض صفحة إضافة دور جديد
        public IActionResult Create()
        {
            return View();
        }

        // إضافة دور جديد
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName, string id="")
        {
            if (string.IsNullOrEmpty(id))
            {
            }
            if (!string.IsNullOrEmpty(roleName))
            {
                var result = await _roleService.CreateRoleAsync(roleName);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "فشل في إنشاء الدور");
            }
            return View();
        }

        // عرض صفحة تعديل الدور
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // تعديل الدور
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string newRoleName)
        {
            if (!string.IsNullOrEmpty(newRoleName))
            {
                var result = await _roleService.UpdateRoleAsync(id, newRoleName);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "فشل في تحديث الدور");
            }
            return View();
        }

        // حذف الدور
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}

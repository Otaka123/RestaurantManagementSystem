using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UsersApp.Services.Repository;

namespace UsersApp.Middleware
{
    public class ValidateOwnershipAttribute:ActionFilterAttribute
    {
         public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // التأكد من وجود المعامل "id" وصلاحيته
            if (context.ActionArguments.TryGetValue("id", out var idObj)
                && idObj is Guid restaurantId
                && restaurantId != Guid.Empty)
            {
                var restaurantService = context.HttpContext.RequestServices.GetService<IRestaurantService>();
                var user = context.HttpContext.User;
                var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!await restaurantService.ValidateOwnership(restaurantId, currentUserId))
                {
                    // إذا كان الـ Controller من نوع Controller نستخدم TempData لتخزين الرسالة
                    if (context.Controller is Controller controller)
                    {
                        controller.TempData["ErrorMessage"] = "You don't have permission to perform this action.";
                    }
                    context.Result = new RedirectToActionResult("Index", "Restaurants", null);
                    return;
                }
            }
            // في حال عدم وجود id صالح (أي عملية إنشاء)، يمكن تخطي التحقق
            await next();
        }
    }
}

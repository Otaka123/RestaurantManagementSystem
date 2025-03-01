using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UsersApp.Middleware
{
    public class JWTTokenMiddleware
    {
       
            private readonly RequestDelegate _next;

            public JWTTokenMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                // قراءة التوكن من الكوكيز
                var token = context.Request.Cookies["AuthToken"];

                if (!string.IsNullOrEmpty(token))
                {
                    // تحليل التوكن وإضافته إلى HttpContext.User (اختياري)
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    var identity = new ClaimsIdentity(jwtToken.Claims, "Jwt");
                    context.User = new ClaimsPrincipal(identity);
                }

                await _next(context);
            }
        
    }
}

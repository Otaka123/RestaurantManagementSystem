using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using UsersApp.Common.Message;
using UsersApp.Data;
using UsersApp.Hubs;
using UsersApp.Middleware;
using UsersApp.Models;
using UsersApp.Models.Seed;
using UsersApp.Profiles;
using UsersApp.Services.Account;
using UsersApp.Services.Cart;
using UsersApp.Services.Dish;
using UsersApp.Services.Email;
using UsersApp.Services.Repository;
using UsersApp.Services.Review;
using UsersApp.Services.Role;
using UsersApp.Validators;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.WebRequestMethods;
using UsersApp.Services.Order;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGenericRepository<DishCategory>, GenericRepository<DishCategory>>();
builder.Services.AddScoped<IGenericRepository<Restaurant>, GenericRepository<Restaurant>>();
builder.Services.AddScoped<IGenericRepository<Dish>, GenericRepository<Dish>>();
builder.Services.AddScoped<IGenericRepository<RestaurantSchedule>, GenericRepository<RestaurantSchedule>>();

builder.Services.AddScoped<IGenericRepository<ShoppingCart>, GenericRepository<ShoppingCart>>();
builder.Services.AddScoped<IGenericRepository<CartDetail>, GenericRepository<CartDetail>>();
builder.Services.AddScoped<IGenericRepository<Order>, GenericRepository<Order>>();
builder.Services.AddScoped<IGenericRepository<OrderDetail>, GenericRepository<OrderDetail>>();
builder.Services.AddScoped<IGenericRepository<OrderStatus>, GenericRepository<OrderStatus>>();


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{

    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartService, CartService>(); 
builder.Services.AddScoped<IOrderService, OrderService>();


builder.Services.AddSignalR();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // مدة صلاحية الكوكيز
    options.SlidingExpiration = true; // تمديد صلاحية الكوكيز عند الاستخدام
});

builder.Services.Configure<IdentityOptions>(opt =>
{
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Lockout.MaxFailedAccessAttempts = 5;
        opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        opt.Lockout.AllowedForNewUsers = true;
    

});
builder.Services.AddAuthentication().AddFacebook(opt =>
{
    opt.ClientId = "906026291696063";
    opt.ClientSecret = "4969d3ca3b36aa4c27cbfb6204872533";
});
builder.Services.AddAuthentication().AddGoogle(opt =>
{
    opt.ClientId = "906026291696063";
    opt.ClientSecret = "4969d3ca3b36aa4c27cbfb6204872533";
});
builder.Services.AddAutoMapper(typeof(CartProfile)); // ضع الـ Profile الصحيح هنا

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await Seed.Initialize(services, roleManager);  // تنفيذ Seed
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseMiddleware<JWTTokenMiddleware>();
app.UseStatusCodePagesWithReExecute("/Home/AccessDenied");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapHub<CartHub>("/cartHub");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

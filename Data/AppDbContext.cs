using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UsersApp.Models;

namespace UsersApp.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
       

        public DbSet<City> Cities { get; set; }
        public DbSet<DishCategory> DishCategories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; } // ✅ تأكد من أن OrderDetails معرفة كخاصية

        public DbSet<OrderStatus> orderStatuses { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantType> RestaurantTypes { get; set; }
        public DbSet<RestaurantSchedule> RestaurantSchedules { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();  // تفعيل التحميل المؤجل

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // ✅ City Table

            modelBuilder.Entity<City>().HasData(
            new City { Id = 1, Name = "Cairo" },
            new City { Id = 2, Name = "Alexandria" },
            new City { Id = 3, Name = "Giza" },
            new City { Id = 4, Name = "Sharkia" },
            new City { Id = 5, Name = "Dakahlia" },
            new City { Id = 6, Name = "Beheira" },
            new City { Id = 7, Name = "Qalyubia" },
            new City { Id = 8, Name = "Monufia" },
            new City { Id = 9, Name = "Gharbia" },
            new City { Id = 10, Name = "Kafr El Sheikh" },
            new City { Id = 11, Name = "Damietta" },
            new City { Id = 12, Name = "Port Said" },
            new City { Id = 13, Name = "Ismailia" },
            new City { Id = 14, Name = "Suez" },
            new City { Id = 16, Name = "Luxor" },
            new City { Id = 17, Name = "Red Sea" },
            new City { Id = 18, Name = "New Valley" },
            new City { Id = 19, Name = "Matrouh" },
            new City { Id = 20, Name = "North Sinai" },
            new City { Id = 21, Name = "South Sinai" },
            new City { Id = 22, Name = "Beni Suef" },
            new City { Id = 23, Name = "Faiyum" },
            new City { Id = 24, Name = "Minya" },
            new City { Id = 25, Name = "Asyut" },
            new City { Id = 26, Name = "Sohag" },
            new City { Id = 27, Name = "Qena" },
            new City { Id = 28, Name = "Aswan" }
       );


            // ✅ RestaurantType Table

            modelBuilder.Entity<RestaurantType>().HasData(
          new RestaurantType { Id = 1, Name = "Syrian Restaurant" },
          new RestaurantType { Id = 2, Name = "Chinese Restaurant" },
          new RestaurantType { Id = 3, Name = "Indian Restaurant" },
          new RestaurantType { Id = 4, Name = "Italian Restaurant" },
          new RestaurantType { Id = 5, Name = "Mexican Restaurant" },
          new RestaurantType { Id = 6, Name = "Lebanese Restaurant" },
          new RestaurantType { Id = 7, Name = "Turkish Restaurant" },
          new RestaurantType { Id = 8, Name = "Japanese Restaurant" },
          new RestaurantType { Id = 9, Name = "American Restaurant" },
          new RestaurantType { Id = 10, Name = "Egyptian Restaurant" },
          new RestaurantType { Id = 11, Name = "Seafood Restaurant" },
          new RestaurantType { Id = 12, Name = "Vegetarian Restaurant" },
          new RestaurantType { Id = 13, Name = "Fast Food Restaurant" },
          new RestaurantType { Id = 14, Name = "Dessert Restaurant" },
          new RestaurantType { Id = 15, Name = "Barbecue Restaurant" },
          new RestaurantType { Id = 16, Name = "Tea House" },
          new RestaurantType { Id = 17, Name = "Healthy Food Shop" },
          new RestaurantType { Id = 18, Name = "Juice Shop" },
          new RestaurantType { Id = 19, Name = "Smoothie Shop" },
          new RestaurantType { Id = 20, Name = "Coffee Shop" },
          new RestaurantType { Id = 21, Name = "Bubble Tea Shop" },
          new RestaurantType { Id = 22, Name = "Ice Cream Parlor" }

      );

            modelBuilder.Entity<OrderStatus>().HasData(
    new OrderStatus { Id = 1, StatusName = "Pending" },
    new OrderStatus { Id = 2, StatusName = "Processing" },
    new OrderStatus { Id = 3, StatusName = "Shipped" },
    new OrderStatus { Id = 4, StatusName = "Delivered" },
    new OrderStatus { Id = 5, StatusName = "Cancelled" },
    new OrderStatus { Id = 6, StatusName = "Returned" }
);

            // ✅ DishCategory Table

            modelBuilder.Entity<DishCategory>().HasData(
          new DishCategory { Id = 1, Name = "Main Course", Description = "الأطباق الرئيسية" },
          new DishCategory { Id = 2, Name = "Appetizers", Description = "المقبلات" },
          new DishCategory { Id = 3, Name = "Desserts", Description = "الحلويات" },
          new DishCategory { Id = 4, Name = "Beverages", Description = "المشروبات" },
          new DishCategory { Id = 5, Name = "Snacks", Description = "السناكس" },
          new DishCategory { Id = 6, Name = "Salads", Description = "السلطات" });

            // تكوين العلاقة بين Restaurant و City
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.City)
                .WithMany(c => c.Restaurants)
                .HasForeignKey(r => r.CityId)
                .OnDelete(DeleteBehavior.Restrict); // منع الحذف التلقائي إذا تم حذف المدينة

            // تكوين العلاقة بين Restaurant و RestaurantType
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.RestaurantType)
                .WithMany(rt => rt.Restaurants)
                .HasForeignKey(r => r.RestaurantTypeId)
                .OnDelete(DeleteBehavior.Restrict); // منع الحذف التلقائي إذا تم حذف النوع

            // تكوين العلاقة بين RestaurantSchedule و Restaurant
            modelBuilder.Entity<RestaurantSchedule>()
                .HasOne(rs => rs.Restaurant)
                .WithMany(r => r.RestaurantSchedules)
                .HasForeignKey(rs => rs.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            // تكوين العلاقة بين Review و Restaurant
            modelBuilder.Entity<Reviews>()
                .HasOne(rv => rv.Restaurant)
                .WithMany(r => r.Reviews)
                .HasForeignKey(rv => rv.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
          

            // تكوين العلاقة بين Review و Dish
            modelBuilder.Entity<Reviews>()
                .HasOne(rv => rv.dish)
                .WithMany(r => r.Reviews)
                .HasForeignKey(rv => rv.Dishid)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CartDetail>()
                .HasOne(cd => cd.restaurant)
                .WithMany(r => r.Cartdetails)
                .HasForeignKey(cd => cd.Restaurantid)
                .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Order>()
            //  .HasOne(o => o.Customer)
            //  .WithMany(u => u.Orders)
            //  .HasForeignKey(o => o.UserId)
            //  .OnDelete(DeleteBehavior.NoAction); 
            // ✅ يمنع الحذف التلقائي
            modelBuilder.Entity<OrderDetail>()
         .HasOne(od => od.restaurant)
         .WithMany(r => r.OrderDetails) // ✅ تأكد أن `OrderDetails` مكتوبة بشكل صحيح في `Restaurant`
         .HasForeignKey(od => od.Restaurantid)
         .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
    
                                         // منع الحذف التلقائي إذا تم حذف النو
                                         //تكوين العلاقة بين Review و Customer(User)
                                         //modelBuilder.Entity<Reviews>()
                                         //        .HasOne(rv => rv.Customer)
                                         //        .WithMany(u => u.Reviews)
                                         //        .HasForeignKey(rv => rv.CustomerId)
                                         //        .OnDelete(DeleteBehavior.NoAction); // ✅ يمنع الحذف التلقائي

            // تكوين العلاقة بين Order و Customer (User)
            //modelBuilder.Entity<Order>()
            //    .HasOne(o => o.Customer)
            //    .WithMany(u => u.Orders)
            //    .HasForeignKey(o => o.UserId)
            //    .OnDelete(DeleteBehavior.Restrict); // منع حذف الطلبات إذا تم حذف المستخدم

            //// تكوين العلاقة بين Order و Restaurant
            //modelBuilder.Entity<Order>()
            //    .HasOne(o => o.Restaurants)
            //    .WithMany(r => r.Orders)
            //    .HasForeignKey(o => o.RestaurantId)
            //    .OnDelete(DeleteBehavior.Restrict); // منع حذف الطلبات إذا تم حذف المطعم

            //// تكوين العلاقة بين OrderItem و Order
            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(oi => oi.Order)
            //    .WithMany(o => o.OrderItems)
            //    .HasForeignKey(oi => oi.OrderId)
            //    .OnDelete(DeleteBehavior.Cascade); // حذف العناصر إذا تم حذف الطلب

            //// تكوين العلاقة بين OrderItem و Dish
            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(oi => oi.Dish)
            //    .WithMany(d => d.OrderItems)
            //    .HasForeignKey(oi => oi.DishId)

            //    .OnDelete(DeleteBehavior.Restrict); // منع حذف العناصر إذا تم حذف الطبق



        }








    }
}

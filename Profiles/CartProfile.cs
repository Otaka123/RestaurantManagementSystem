using AutoMapper;
using UsersApp.Models;
using UsersApp.ViewModels;
using UsersApp.ViewModels.Cart;

namespace UsersApp.Profiles
{
    public class CartProfile : Profile
    {
        public CartProfile() {

            CreateMap<ShoppingCart, CartViewModel>()
                .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.Id))
                //.ForMember(dest => dest.IsGuestCart, opt => opt.MapFrom(src => src.IsGuestCart))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.CartDetails.Count))
                .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.CartDetails.Sum(c => c.Quantity * c.UnitPrice)))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartDetails));

            CreateMap<CartDetail, CartItemViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Dishid, opt => opt.MapFrom(src => src.DishId))
                .ForMember(dest => dest.DishName, opt => opt.MapFrom(src => src.Dish.Name))
                //.ForMember(dest => dest.Restaurantid, opt => opt.MapFrom(src => src.Restaurantid))
                //.ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Dish.Picture))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Dish.Description))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

            //CreateMap<Order, OrderViewModel>()
            //   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            //   .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            //   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //   .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            //   .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
            //   .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            //   .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
            //   // إذا كنت تريد تحويل حالة الطلب إلى نص مثلاً
            //   .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.OrderStatus != null ? src.OrderStatus.StatusName : "غير محدد"))
            //   .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetail));

            //CreateMap<OrderDetail, OrderDetailViewModel>()
            //    .ForMember(dest => dest.DishId, opt => opt.MapFrom(src => src.DishId))
            //    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            //    .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
        
    }
    }
}

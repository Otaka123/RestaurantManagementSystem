using Microsoft.AspNetCore.SignalR;

namespace UsersApp.Hubs
{
    public class CartHub:Hub
    {
        public async Task UpdateCartCount(int count)
        {
            await Clients.All.SendAsync("ReceiveCartCount", count);
        }
    }
}

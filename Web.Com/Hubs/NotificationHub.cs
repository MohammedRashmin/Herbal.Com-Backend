using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Web.Com.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // SignalR automatically maps the user based on the NameIdentifier claim 
        await base.OnConnectedAsync();
    }
}

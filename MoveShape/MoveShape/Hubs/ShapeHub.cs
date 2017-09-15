using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MoveShape.Hubs
{
    public class ShapeHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task MoveShape(int x , int y)
        {
            //This will be simpler once there is support for Others or when AllExcept has a params overload
            var excludedIds = new List<string>();
            excludedIds.Add(Context.ConnectionId);
            await Clients.AllExcept(excludedIds).InvokeAsync("shapeMoved", x, y);
        }
    }
}

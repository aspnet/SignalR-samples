using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhiteBoard.Hubs
{
    public class DrawHub : Hub
    {
        public Task MouseUp()
        {
            return Clients.AllExcept(new List<string> { Context.ConnectionId }).InvokeAsync("mouse-up");
        }

        public Task MouseDown()
        {
            return Clients.AllExcept(new List<string> { Context.ConnectionId }).InvokeAsync("mouse-down");
        }

        public Task MouseOut()
        {
            return Clients.All.InvokeAsync("mouse-out");
        }

        public Task Draw(int x, int y, string color)
        {
            return Clients.AllExcept(new List<string> { Context.ConnectionId }).InvokeAsync("draw", x, y, color);
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhiteBoard.Hubs
{
    public class DrawHub : Hub
    {
        public Task Draw(int prevX, int prevY, int x, int y, string color)
        {
            return Clients.AllExcept(new List<string> { Context.ConnectionId }).InvokeAsync("draw", prevX, prevY, x, y, color);
        }
    }
}

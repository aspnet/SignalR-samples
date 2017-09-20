using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhiteBoard.Hubs
{
    public class DrawHub : Hub
    {
        public Task Draw(int prevX, int prevY, int currentX, int currentY, string color)
        {
            return Clients.AllExcept(new List<string> { Context.ConnectionId }).InvokeAsync("draw", prevX, prevY, currentX, currentY, color);
        }
    }
}

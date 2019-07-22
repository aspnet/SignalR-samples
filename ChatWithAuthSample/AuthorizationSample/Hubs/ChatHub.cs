using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationSample.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static Dictionary<string, string> UserRoster = new Dictionary<string, string>();

        /// <summary>
        /// Handles the connection of a user.
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                // Checking to see if new user or just new connection.
                bool isUserNew = !UserRoster.ContainsValue(Context.User.Identity.Name);

                // Add user to roster.
                lock(UserRoster)
                {
                    UserRoster.Add(Context.ConnectionId, Context.User.Identity.Name);
                }

                // Inform rest of group only if new user, not if new connection.
                if (isUserNew)
                    await Clients.Groups("broadcast-group").SendAsync("add-user", Context.User.Identity.Name);

                // Join broadcast group.
                await Groups.AddToGroupAsync(Context.ConnectionId, "broadcast-group");
                // Tell user about roster. Letting the connection complete while this is happening.
                Clients.Caller.SendAsync("users", UserRoster.Values.ToList());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await base.OnConnectedAsync();
            }
        }

        /// <summary>
        /// Handles the disconnection of a user.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                // Remove connection from UserRoster.
                lock(UserRoster)
                {
                    UserRoster.Remove(Context.ConnectionId);
                }

                // If user has no other connections, inform the group that they have left.
                if (!UserRoster.ContainsValue(Context.User.Identity.Name))
                    await Clients.Groups("broadcast-group").SendAsync("remove-user", Context.User.Identity.Name);

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }

        /// <summary>
        /// Sends message to the group.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Send(Models.MessageModel model)
        {
            try
            {
                // Set UserName on message model.
                model.UserName = Context.User.Identity.Name;
                await Clients.Group("broadcast-group").SendAsync("message", model);
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}

using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace APIs.Utils.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // update unread chat messages count

        public async Task SendMessage(string receiverId, string message)
        {
            // Implement logic to save the message to the database
            // You can use _context to interact with the database

            // Broadcast the message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", message);
        }


        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class NameUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        }
        public virtual string GetConnectionId(HubConnectionContext connection)
        {
            return connection.ConnectionId;
        }
    }
}

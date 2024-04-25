using APIs.Utils.Hubs;
using APIs.Services.Interfaces;
using Azure;
using BusinessObjects;
using BusinessObjects.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace APIs.Services.Interfaces // Remember to replace this with your actual namespace
{
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hub;
        private readonly IAccountService _accService;

        public ChatController(AppDbContext context, IHubContext<ChatHub> hub, IAccountService accService)
        {
            _context = context;
            _hub = hub;
            _accService = accService;
        }

        // GET: api/Chat/GetChatUsers/5
        [HttpGet("GetChatUsers")]
        public async Task<ActionResult<IEnumerable<UserChatDTO>>> GetChatUsers()
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest("User is not authenticated");
            }
            var userChatDTOs = await _context.ChatMessages
                .Where(m => m.SenderId == userId)
                .Select(m => m.Receiver)
                .Distinct()
                .Select(receiver => new UserChatDTO
                {
                    UserId = receiver.UserId,
                    Username = receiver.Username,
                    Avatar = receiver.AvatarDir
                })
                .ToListAsync();
            return userChatDTOs;
        }

        [HttpGet("GetChatMessages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDTO>>> GetChatMessages()
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest("User is not authenticated");
            }
            var users = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.Receiver : m.Sender)
                .Distinct()
                .ToListAsync();
            var chatMessages = new List<ChatMessageDTO>();
            var processedUserIds = new HashSet<Guid>();
            foreach (var user in users)
            {
                if (processedUserIds.Contains(user.UserId))
                    continue;
                processedUserIds.Add(user.UserId);
                var userChatMessages = await _context.ChatMessages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == user.UserId) ||
                                (m.SenderId == user.UserId && m.ReceiverId == userId))
                    .OrderBy(m => m.SentOn)
                    .Select(m => new UserChatMessageDTO
                    {
                        Id = m.Id,
                        MessageText = m.MessageText,
                        SentDate = m.SentOn,
                        SenderId = m.SenderId,
                        Avatar = m.Sender.AvatarDir,
                        Username = m.Sender.Username,
                    })
                    .ToListAsync();
                var messageDTO = new ChatMessageDTO
                {
                    UserId = user.UserId,
                    ChatHistory = userChatMessages,
                    Avatar = user.AvatarDir,
                    Username = user.Username,
                };
                chatMessages.Add(messageDTO);
            }
            return chatMessages;
        }

        // POST: api/Chat/PostChatMessage/5
        [HttpPost("PostChatMessage")]
        public async Task<IActionResult> PostChatMessage(NewMessageDTO model)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest("User is not authenticated");
            }
            var userinfo = await _accService.GetUserById(userId);
            var newMessage = new ChatMessage
            {
                SenderId = userId,
                ReceiverId = model.ReceiverId,
                MessageText = model.MessageText,
                SentOn = DateTime.Now,
            };
            _context.ChatMessages.Add(newMessage);
            await _context.SaveChangesAsync();
            var repspone = new MessageReceivedDTO
            { 
                Id = newMessage.Id,
                SenderId = userId,
                MessageText = model.MessageText,
                SentDate = newMessage.SentOn,
                Avatar = newMessage.Sender?.AvatarDir,
                Username = newMessage.Sender?.Username,
            };
            await _hub.Clients.User(model.ReceiverId.ToString()).SendAsync("NewMessageReceived", newMessage);

            return Ok(repspone);
        }
    }
}

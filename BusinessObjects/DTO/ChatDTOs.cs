using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace BusinessObjects.DTO
{
    public class UserChatDTO
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string? Avatar { get; set; }

    }

    public class ChatMessageDTO
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string? Avatar { get; set; }
        public string? Username { get; set; }
        public List<UserChatMessageDTO>? ChatHistory { get; set; } // Change to List<ChatMessageDTO>
    }
    public class NewMessageDTO
    {
        public Guid ReceiverId { get; set; }
        public string MessageText { get; set; }
    }
    public class MessageReceivedDTO
    {
        public int Id { get; set; }
        public Guid SenderId { get; set; }
        public DateTime SentDate { get; set; }

        public string? MessageText { get; set; }
        public string? Avatar { get; set; }
        public string? Username { get; set; }
    }
    public class UserChatMessageDTO
    {
        public int Id { get; set; }
        public Guid SenderId { get; set; }
        public string? MessageText { get; set; }
        public DateTime SentDate { get; set; }
        public string? Avatar { get; set; }
        public string? Username { get; set; }
    }
}

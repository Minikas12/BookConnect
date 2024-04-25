namespace BusinessObjects.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string? MessageText { get; set; }
        public DateTime SentOn { get; set; }
        public AppUser Sender { get; set; } = null!;
        public AppUser Receiver { get; set; } = null!;
    }
}

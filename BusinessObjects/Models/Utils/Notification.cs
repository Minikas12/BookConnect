using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.Utils
{
    public class Notification
    {
        [Key]
        public Guid NotiId { get; set; }
        public Guid AnnouncerId { get; set; }
        public Guid ReceiverId { get; set; }
        public string? MessageText { get; set; }
        public DateTime SentOn { get; set; }
        public AppUser Announcer { get; set; } = null!;
        public AppUser? Receiver { get; set; }
    }
}

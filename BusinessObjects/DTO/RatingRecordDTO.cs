using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTO
{
    public class RatingRecordDTO
    {
        public Guid RatingRecordId { get; set; }
        public Guid RatingId { get; set; }
        public Guid UserId { get; set; }
        public string? RatingPoint { get; set; }
        public string? Comment { get; set; }
      
    }

    public class PointandIdDTO
    {
        public Guid RatingId { get; set; }
        public double OverallRating { get; set; } 
    }
    public class CommentWithUserDTO
    {
        public Guid RatingRecordId { get; set; }
        public Guid? RatingId { get; set; }
        public Guid? ReviewerId { get; set; }
        public int RatingPoint { get; set; }
        public string? Comment { get; set; }
        public CommentReplyDTO? Replies { get; set; } // List of replies
        public DateTime? CreatedDate { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? AvatarDir { get; set; }
    }

    public class CommentReplyDTO
    {
        public Guid ReplyId { get; set; }
        public string? ReplyText { get; set; }
        public Guid AgencyId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? AgencyName { get; set; }
        public string? LogoUrl { get; set; }
    }

    public class ReplyAddDTO
    {
        public Guid RatingRecordId { get; set; }
        public Guid AgencyId { get; set; }
        public string? ReplyText { get; set; }
    }

    public class ReplyUpdateDTO
    {
        public Guid ReplyId { get; set; }
        public Guid RatingRecordId { get; set; }
        public Guid AgencyId { get; set; }
        public string? ReplyText { get; set; }
    }
    public class ReviewDTO //SONDB
    {
        public Guid RatingRecordId { get; set; }
        public Guid RatingId { get; set; }
        public int RatingPoint { get; set; }
        public Guid BookId { get; set; }
        public string? BookName { get; set; }
        public Guid UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Comment { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarDir { get; set; }
        public CommentReplyDTO? Reply { get; set; }
    }
    public class ReviewSearchCriteria //SONDB
    {
        public string? BookName { get; set; }
        public string? RatingPoint { get; set; }
        public string? HasReplied { get; set; }
        public string? RecentDays { get; set; }
    }
}

using BusinessObjects.Enums;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTO.Trading
{
//-------------------------------------------------POST-----------------------------------------------------//
 
    public class AddPostDTOs
    {
        public string Title { get; set; } = null!;
        public IFormFile? ProductImages { get; set; }
        public IFormFile? ProductVideos { get; set; }
        public string? Content { get; set; }
        public bool IsTradePost { get; set; }
        public List<Guid>? CateIds { get; set; }
    }


    public class UpdatePostDTOs
    {
        public Guid PostId { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Video { get; set; }
        public string? Title { get; set; } = null!;
        public string? Content { get; set; }
        public bool? IsLock { get; set; }
        //public DateTime LastUpdate { get; set; }
        //public List<string> CateId { get; set; } = null!;
    }

    public class PostDetailsDTO
    {
        public Post PostData { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AvatarDir { get; set; }
        public string? ReadingTime { get; set; } 
        public List<CateNameAndIdDTO>? Tags { get; set; }
    }
    

    //-------------------------------------------------COMMENT-----------------------------------------------------//

    public class CommentDetailsDTO
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public Guid CommenterId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreateDate { get; set; } 
        public string Username { get; set; } = null!;
        public string? AvatarDir { get; set; }
    }

    public class AddCommentDTO
    {
        public Guid PostId { get; set; }
        public Guid CommenterId { get; set; }
        public string Content { get; set; } = null!;
    }

    public class AddCommentReturnDTO
    {
        public Comment CmtData { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string AvatarDir { get; set; } = null!;
    }

    //public class UpdateCommentDTO
    //{
    //    public Guid CommentId { get; set; }
    //    public Guid PostId { get; set; }
    //    public Guid CommenterId { get; set; }
    //    public string Content { get; set; }
    //}
    //-------------------------------------------------POST INTEREST-----------------------------------------------------//
    public class InteresterDetailsDTO
    {
        public Guid RecordId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarDir { get; set; }
        public bool IsChosen { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class AcceptTradeDTO
    {
        public Guid PostId { get; set; }
        public Guid InteresterId { get; set; }
    }

    public class TradeStatusUpdateDTO
    {
        public Guid PostId { get; set; }
        public Guid TradeDetailsId { get; set; }
        public TradeStatus UpdatedStatus { get; set; }
    }

    public class AddPostInterestDTO
    {
        public Guid PostId { get; set; }
    }

    public class UpdatePostInterestDTO
    {
        public Guid PostId { get; set; }
        public Guid PostInterestId { get; set; }
        public Guid InteresterId { get; set; }
    }

    public class DeletePostInterestDTO
    {
        public Guid InteresterId { get; set; }
        public Guid PostId { get; set;}
    }

}
using BusinessObjects.Enums;
using BusinessObjects.Models.Trading;
using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO.Trading
{
       public class SubmitTradeDetailDTO
        {
            public Guid PostId { get; set; }
            public Guid TradeDetailsId { get; set; }
            public bool IsPostOwner { get; set; }
            public string? City_Province { get; set; }
            public string? District { get; set; }
            public string? SubDistrict { get; set; }
            public string Rendezvous { get; set; } = null!;
            public string? Phone { get; set; }
            public string? Note { get; set; }
        }

        public class UpdateTradeDetailsDTO
        {
            public Guid TradeDetailId { get; set; }
            public Guid? AddressId { get; set; }
            public string? Phone { get; set; }
            public string? Note { get; set; }
            public TradeStatus Status { get; set; }
        }

        public class PostTradeRatingDTO
        {
            public Guid RevieweeId { get; set; }
            public Guid TradeDetailsId { get; set; }
            public string? Comment { get; set; }
            public int RatingPoint { get; set; }
        }

        public class GetTradeDetailsByPostIdDTO
        {
            public TradeDetails Details { get; set; } = null!;
            public Guid TraderId { get; set; }
        }

        public class GetTraderIdsByPostIdDTO
        {
            public Guid OwnerId { get; set; }
            public Guid InteresterId { get; set; }
        }

        public class GetLockedPostsByUserIdDTO
        {
            public Guid PostId { get; set; }
            public string Status { get; set; } = null!;
            public string Title { get; set; } = null!;
            public DateTime CreateDate { get; set; }
        }

        public class MiddleManAddressDTO
        {
            public Guid AddressId { get; set; }
            public string Address { get; set; } = null!;
        }
        //---------------------------------------------Book Check List-------------------------------------------------------//
        public class AddBookCheckListRequestDTO
        {
            public Guid TradeDetailsId { get; set; }
            public List<string>? Targets { get; set; }
        }
        public class UpdateCheckListSubmitDTO
        {
            public List<CheckListUpdateDTO> Data { get; set; } = null!;
        }
        public class CheckListUpdateDTO
        {
            public Guid Id { get; set; }
            public string Target { get; set; } = null!;
            public Guid TradeDetailsId { get; set; }
            public IFormFile? BookOwnerUploadDir { get; set; }
            public IFormFile? MiddleUploadDir { get; set; }
        }
}
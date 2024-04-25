using BusinessObjects.Enums;
using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO
{
	public class NewAdDTO
	{
		public Guid AgencyId { get; set; }
		public string Duration { get; set; } = null!;
		public int? NumberOfTargetUser { get; set; }
		public decimal? PPC_Price { get; set; }
		public CampaignType CampaignType { get; set; }
		public Guid? BookId { get; set; }
		public string? BannerImg {get; set;}
		public decimal? DisplayBid { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class AdBillDTO
    {
        public Guid TransactionId { get; set; } 
        public CampaignType CampaignType { get; set; } //0=Display,1=Recommend
        public string Duration { get; set; } = null!;
        public int? NumberOfTargetUser { get; set; }
        public decimal? PPC_Price { get; set; }
        public Guid? BookId { get; set; }
        public decimal? DisplayBid { get; set; }
    }

    public class TopBannerDTO
    {
        public Guid AdId { get; set; }
        public string? BannerTitle { get; set; }
        public decimal Price { get; set; }
        public string BannerDir { get; set; } = null!;
        public Guid? ProductId { get; set; }
        public Guid? AgencyId { get; set; } 
    }

    public class RelevantBookDTO
    {
        public Guid BookId { get; set; }
        public string ImageDir { get; set; } = null!;
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public double Rating { get; set; }
    }
}


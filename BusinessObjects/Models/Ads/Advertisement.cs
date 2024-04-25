using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Enums;
using BusinessObjects.Models.Ecom.Payment;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Ads
{
	public class Advertisement
	{
        //Id, AgencyId, Image(nullable), ProductId, Campaign(search, display, recommend),
        //Bid: Ad_Id, PerClick(double), PerAnticipated()
        //Quality_score:Ad_Id, numberOfClick, relevant keyword
        [Key]
        public Guid AdId { get; set; }
        public CampaignType CampaignType { get; set; }

        public string? BannerDir { get; set; }
        public decimal BannerFee { get; set; }

        public decimal TargetUserFee { get; set; }
        public decimal PPC_Price { get; set; }

        public string Duration { get; set; } = null!;
        public decimal SubFee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Guid? AgencyId { get; set; }
        [ForeignKey("AgencyId"), JsonIgnore]
        public Agency? Agency { get; set; }

        public Guid? BookId { get; set; }
        [ForeignKey("BookId"), JsonIgnore]
        public Book? Book { get; set; }

        public Guid TransactionId { get; set; }
        public TransactionRecord Transaction { get; set; } = null!;
    }
}


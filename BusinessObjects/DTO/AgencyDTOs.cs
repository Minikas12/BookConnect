using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO
{
	public class NameAndIdDTO
	{
		public Guid AgencyId { get; set; }
		public string AgencyName { get; set; } = null!;
	}

    public class AgencyRegistrationDTO
    {
        public string AgencyName { get; set; } = null!;
        public IFormFile? LogoImg { get; set; }
        public Guid AddressId { get; set; }
        //public string? City_Province { get; set; }
        //public string? District { get; set; }
        //public string? SubDistrict { get; set; }
        //public string Rendezvous { get; set; } = null!;
        //public bool Default { get; set; } = false;
        public string BusinessType { get; set; } = null!;
    }

    public class AgencyUpdateDTO
	{
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = null!;
        public Guid OwnerId { get; set; }
        public string? PostAddress { get; set; }
        public IFormFile? LogoImg { get; set; }
        public string BusinessType { get; set; } = null!;
    }

    // --------------------------------------------DATDQ----------------------------------//

    public class RevenueInfo
    {
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SoldInfo
    {
        public int NumberOfBookSold { get; set; }
        public int NumberOfUnitSold { get; set; }
    }
    public class AgencyAnalystDTO
    {
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = null!;
        public int? TotalQuantityOfBookInInventory { get; set; }
        public int? TotalBookSold { get; set; }
        public int? TotalUnitSold { get; set; }
        public Decimal? TotalRevenue { get; set; }
        public Decimal? ThisMonthRevenue { get; set; }
        public Decimal? ThisDayRevenue { get; set; }
        public Decimal? AvgMonthRevenue { get; set; }
        public Decimal? AvgDayRevenue { get; set; }
        public string? HighestMonthRevenue { get; set; }
        public string? HighestDayRevenue { get; set; }
        public Decimal? PercentThisMonthToAvgMonth { get; set; }
        public Decimal? PercentThisDayToAvgDay { get; set; }
        public Decimal? PercentThisMonthToHighestMonth { get; set; }
        public Decimal? PercentThisDayToHighestDay { get; set; }
        public Dictionary<string, decimal>? RevenueByMonths { get; set; }
        public Dictionary<string, decimal>? RevenueByDays { get; set; }
        public Dictionary<string, RevenueInfo>? RevenueByType { get; set; }
        public Dictionary<string, RevenueInfo>? RevenueByCategory { get; set; }
        public Dictionary<string, SoldInfo>? NumberOfBookAndUnitSoldByMonths { get; set; }
        public Dictionary<string, SoldInfo>? NumberOfBookAndUnitSoldByDays { get; set; }
    }

    public class AgencyAnalystTimeInputDTO
    {
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = null!;
        public Decimal? Revenue { get; set; }
        public Dictionary<string, decimal>? RevenueByTimeInput { get; set; }
        public Dictionary<string, SoldInfo>? NumberOfBookAndUnitSoldByTimeInput { get; set; }
    }

    // ----------------------------------------END DATDQ----------------------------------//
   
}


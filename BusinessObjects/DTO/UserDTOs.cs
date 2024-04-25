using System;
using System.ComponentModel.DataAnnotations;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO
{
	public class UserProfileDTO
	{
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AvatarDir { get; set; }
        public List<string>? Roles { get; set; }
        public string? Address { get; set; }
        public bool IsValidated { get; set; }
        public bool IsSeller { get; set; }
        public bool IsBanned { get; set; }
        public List<Agency>? Agencies { get; set; }
        //public string Rating { get; set; } = null!;
    }

    public class UserValidationDTO
    {
        public Guid UserId { get; set; }
        //"ctcId": "046202000515",
      public string NicId { get; set; } = null!;
        //"ctcName": "NGUYỄN ĐÌNH TRUNG",
      public string NicName { get; set; } = null!;
        //"ctcDob": "2002-11-17T17:00:00.000Z",
      public string NicDob { get; set; } = null!;
        //"ctcHome": "THỦY PHƯƠNG, HƯƠNG THỦY, THỪA THIÊN HUẾ",
      public string NicHome { get; set; } = null!;
        //"ctcSex": "NAM",
        public string NicSex { get; set; } = null!;
        //"ctcNationality": "VIỆT NAM",
        public string NicNationality { get; set; } = null!;
        //"ctcDoe": "2027-11-17T17:00:00.000Z"
        public string NicDoe { get; set; } = null!;
    }

    public class BanUserDTO
    {
        public Guid UserId { get; set; }
        public string Reason{ get; set; } = null!;
        public TimeSpan? Duration { get; set; }
    }

    public class NewAddressDTO
    {
        public Guid AddressId { get; set; }
        public string? City_Province { get; set; }
        public string? District { get; set; } 
        public string? SubDistrict { get; set; }
        public string Rendezvous { get; set; } = null!;
        public bool Default { get; set; } = false; 
        public Guid? UserId { get; set; }
    }

    public class CompareAddressDTO
    {
        public string? City_Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string Rendezvous { get; set; } = null!;
        public bool Default { get; set; } = false;
        public Guid? UserId { get; set; }
    }
    public class UserProfile
    {
        public Guid UserId { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        public string? Username { get; set; }
        
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? Phone { get; set; }
        
        public IFormFile? AvatarDir { get; set; }
    }
    public class ProfileUserDTO
    {
        public Guid UserId { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }

        public string? AvatarDir { get; set; }
    }

    public class UserPublicProfileDTO
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AvatarDir { get; set; }
    }


    public class UserOrderHistoryDTO
    {
        public Guid OrderId { get; set; }
        public decimal? OrderPrice { get; set; }
        public Guid BookId { get; set; }
        public string? BookName { get; set; }
        public string? BookDir { get; set; }
        public decimal BookPrice { get; set; }
        public int BookQuantity { get; set; }
        public Guid AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public DateTime OrderTime { get; set; }
    }
    public class RatingResponseDTO
    {
        public Guid RatingRecordId { get; set; }
        public Guid RatingId { get; set; }
        public Guid ReviewerId { get; set; }
        public int RatingPoint { get; set; }
        public string Comment { get; set; } = null!;
        public List<object> Replies { get; set; } = new List<object>();
        public DateTime CreatedDate { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AvatarDir { get; set; }
    }
    public class VerificationRequest
    {
        public string Email { get; set; }
        public Guid UserId { get; set; }
    }
}


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.Ecom.Rating;
using Newtonsoft.Json;

namespace BusinessObjects.Models
{
	public class AppUser
	{
        [Key]
        public Guid UserId { get; set; }
		public string Username { get; set; } = null!;
		public string Email { get; set; } = null!;
		public string Password { get; set; } = null!;
		public string Salt { get; set; } = null!;
        public string? Phone { get; set; }
        public string? AvatarDir { get; set; }
		public bool IsValidated { get; set; } = false;
		public bool IsBanned { get; set; } = false;
		public double OverallRating { get; set; }
        //public bool IsEmailVerify { get; set; } = false;

    }
}


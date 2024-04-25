using System;
namespace BusinessObjects.DTO
{
	public class AddPostToCategoryDTO
	{
		public Guid PostId { get; set; } 
		public List<Guid> CategoryIds { get; set; } = null!;
	}

	public class RemovePostfromCategoryDTO
	{
		public Guid PostId { get; set; }
		public Guid CategoryId { get; set; }
	}

	public class GetUTCByUserIdResponseDTO
	{
		public Guid CateId { get; set; }
		public string? CateName { get; set; } 
	}

	public class AddPostSocialTagRequestDTO
	{
		public List<string> TagNames { get; set; } = null!;
		public Guid PostId { get; set; }
    }
}


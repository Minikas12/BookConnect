﻿using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO
{
	public class NewCateDTO //DATDQ
    {
		public Guid? CateId { get; set; }
		public string? CateName { get; set; } = null!;
		public IFormFile? CateImg { get; set; }
		public string? Description { get; set; }
	}

	public class UploadCloudinaryDTO
	{
		public IFormFile file { get; set; } = null!;
	}

	public class CateNameAndIdDTO
	{
		public Guid CateId { get; set; }
		public string CateName { get; set; } = null!;
	}
}


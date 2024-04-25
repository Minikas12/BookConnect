using System;
using BusinessObjects.DTO;

namespace APIs.Services.Interfaces
{
	public interface ICloudinaryService
	{
        CloudinaryResponseDTO UploadImage(IFormFile imgFile, string dir);
        CloudinaryResponseDTO UploadVideo(IFormFile imgFile, string dir);
        CloudinaryResponseDTO DeleteImage(string imgUrl, string type);
        CloudinaryResponseDTO DeleteVideo(string vidUrl, string type);
    }
}


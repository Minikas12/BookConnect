using System;
using CloudinaryDotNet;
using APIs.Services.Interfaces;
using BusinessObjects.DTO;
using CloudinaryDotNet.Actions;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using BusinessObjects.Models;

namespace APIs.Services
{
	public class CloudinaryService: ICloudinaryService
	{
        private readonly IConfiguration _config;
        private readonly Account account;

        public CloudinaryService(IConfiguration config)
		{
            _config = config;
            account = new Account(
                 config.GetSection("Cloudinary")["CloudName"],
                 config.GetSection("Cloudinary")["ApiKey"],
                 config.GetSection("Cloudinary")["ApiSerect"]
                );
        }
        //---------------------------------------------------------IMAGE---------------------------------------------------------//

        public CloudinaryResponseDTO UploadImage(IFormFile uFile, string dir)
        {
            var client = new Cloudinary(account);
            var imageuploadParams = new ImageUploadParams()
            {
                Folder = dir,
                File = new FileDescription(uFile.FileName, uFile.OpenReadStream()),
                DisplayName = uFile.FileName
            };
            var uploadResult = client.Upload(imageuploadParams);
            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)uploadResult.StatusCode,
                    Message = uploadResult.Error.Message
                };
            }
            if (uploadResult == null)
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Undefied error!"
                };
            }
            return new CloudinaryResponseDTO()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Upload successful!",
                Data = uploadResult.SecureUrl.ToString()
            };
        }

        //type is what obj does the video belong to
        public bool IsImageExisted(string imgUrl, string type)
        {
            var client = new Cloudinary(account);
            string publicId = "";
            switch (type)
            {
                case "Category":
                    publicId = Regex.Match(imgUrl, $@"{account.Cloud}/image/upload/v\d+/(.*)\.\w+").Groups[1].Value;
                    break;
                case "Post":
                    publicId = Regex.Match(imgUrl, $@".*?/v\d+/(.+?).[^.]+$").Groups[1].Value; ;
                    break;
                case "Ad":
                    publicId = Regex.Match(imgUrl, $@".*?/v\d+/(.+?).[^.]+$").Groups[1].Value; ;
                    break;
            }
                
            GetResourceResult resourceResult = client.GetResource(publicId);

            return (resourceResult != null && resourceResult.StatusCode == HttpStatusCode.OK);
        }

        //type is what obj does the video belong to
        public CloudinaryResponseDTO DeleteImage(string imgUrl, string type)
        {
            var client = new Cloudinary(account);
            string publicId = "";
            switch (type)
            {
                case "Category":
                    publicId = Regex.Match(imgUrl, $@"{account.Cloud}/image/upload/v\d+/(.*)\.\w+").Groups[1].Value;
                    break;
                case "Post":
                    publicId = Regex.Match(imgUrl, $@".*?/v\d+/(.+?).[^.]+$").Groups[1].Value; 
                    break;
            }
            //Directory base regex
            if (string.IsNullOrEmpty(publicId)) //Fix To update profile SONDB
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "PublicId not found in the image URL"
                };
            DeletionParams deletionParams = new DeletionParams(publicId);

            var result = client.Destroy(deletionParams);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)result.StatusCode,
                    Message = result.Error.Message
                };
            }
            if (result == null)
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Undefied error!"
                };
            }
            return new CloudinaryResponseDTO()
            {
                StatusCode = (int)result.StatusCode,
                Message = "Delete successful!",
            };
        }
        //---------------------------------------------------------VIDEO---------------------------------------------------------//

        // Upload Video
        public CloudinaryResponseDTO UploadVideo(IFormFile uFile, string dir)
        {
            var client = new Cloudinary(account);
            var VideoUploadParams = new VideoUploadParams()
            {
                Folder = dir,
                File = new FileDescription(uFile.FileName, uFile.OpenReadStream()),
                DisplayName = uFile.FileName,
            };
            var uploadResult = client.Upload(VideoUploadParams);
            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)uploadResult.StatusCode,
                    Message = uploadResult.Error.Message
                };
            }
            if (uploadResult == null)
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Undefied error!"
                };
            }
            return new CloudinaryResponseDTO()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Upload successful!",
                Data = uploadResult.SecureUrl.ToString()
            };
        }

        //Check existed video
        public bool IsVideoExisted(string vidUrl, string type)
        {
            var client = new Cloudinary(account);
            string publicId = "";
            switch (type)
            {
                case "Category":
                    publicId = Regex.Match(vidUrl, $@"{account.Cloud}/video/upload/v\d+/(.*)\.\w+").Groups[1].Value;
                    break;
                case "Post":
                    publicId = Regex.Match(vidUrl, $@".*?/v\d+/(.+?)(\.[^.]+)$").Groups[1].Value;
                    break;
            }
            GetResourceResult resourceResult = client.GetResource(publicId);

            return (resourceResult != null && resourceResult.StatusCode == HttpStatusCode.OK);
        }

        // Delete Video, type is what obj does the video belong to
        public CloudinaryResponseDTO DeleteVideo(string vidUrl, string type)
        {
            var client = new Cloudinary(account);

            if (string.IsNullOrWhiteSpace(vidUrl) || string.IsNullOrWhiteSpace(type))
            {
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid vidUrl or type provided."
                };
            }

            string publicId = "";

            switch (type)
            {
                case "Category":
                    publicId = Regex.Match(vidUrl, $@"{account.Cloud}/video/upload/v\d+/(.*)\.\w+").Groups[1].Value;
                    break;
                case "Post":
                    publicId = Regex.Match(vidUrl, $@".*?/v\d+/(.+?)(\.[^.]+)$").Groups[1].Value;
                    break;
                default:
                    return new CloudinaryResponseDTO()
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Invalid type provided."
                    };
            }

            try
            {
                DeletionParams deletionParams = new DeletionParams("Posts/Phuong Uyen/6344e0d0-b53a-4caa-8a6e-1ca347c97ae1/Videos/amse8eumzzk6hi1shov6");
                var result = client.Destroy(deletionParams);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new CloudinaryResponseDTO()
                    {
                        StatusCode = (int)result.StatusCode,
                        Message = result.Error?.Message ?? "Unknown error occurred."
                    };
                }

                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)result.StatusCode,
                    Message = "Delete successful!" + publicId
                };
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the deletion process
                return new CloudinaryResponseDTO()
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }
    }
}


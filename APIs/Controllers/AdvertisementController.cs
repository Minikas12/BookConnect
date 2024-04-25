using APIs.Services.Interfaces;
using BusinessObjects.DTO;
using BusinessObjects.Enums;
using BusinessObjects.Models.Ads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIs.Controllers
{
	[ApiController]
	[Route("api/ad")]
	public class AdvertisementController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IBookService _bookService;
		private readonly ICloudinaryService _cloudinaryService;
		private readonly ITransactionService _transactionService;

		public AdvertisementController(IUnitOfWork unitOfWork, IBookService bookService, ICloudinaryService cloudinaryService, ITransactionService transactionService)
		{
			_unitOfWork = unitOfWork;
			_transactionService = transactionService;
			_bookService = bookService;
			_cloudinaryService = cloudinaryService;
		}

		[HttpPost("save-banner-to-cloudinary")]
		public IActionResult SaveBannerToCloudinary([FromBody] string base64String)
		{
            byte[] fileBytes = Convert.FromBase64String(base64String);

            MemoryStream memoryStream = new MemoryStream(fileBytes);

            IFormFile formFile = new FormFile(memoryStream, 0, fileBytes.Length, null, "Siuuu")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            //return formFile;
            var saveProductResult = _cloudinaryService.UploadImage(formFile, "Banner/Images");
            if (saveProductResult.StatusCode != 200 || saveProductResult.Data == null)
            {
                return BadRequest(saveProductResult.Message);
            }
            var imageDir = saveProductResult.Data;
			return Ok(imageDir);
        }

        [HttpPost("register-new-ad"), Authorize]
		public async Task<IActionResult> AddNewAdvertisement([FromForm] NewAdDTO dto)
		{
			decimal costPerTargetUser = 1000;
			decimal subFee = 0;
			int duration = 0;

			decimal PPC_Price = dto.PPC_Price ?? 0;
			decimal numberOfTargetUser = dto.NumberOfTargetUser ?? 0;
			decimal displayBid = dto.DisplayBid ?? 0;

			switch (dto.Duration)
			{
				case "Week":
					duration = 7; subFee = 200000;
					break;
				case "Month":
					duration = 30; subFee = 700000;
					break;
				case "Year":
					duration = 365; subFee = 8400000;
					break;
			}
			Guid adId = Guid.NewGuid();

			if (!ModelState.IsValid)
			{
				return BadRequest("Model invalid!");
			}

			if(await _unitOfWork.AdsService.IsTransactionBennAssigned(dto.TransactionId))
			{
				return BadRequest("Transaction's already been assigned to another record!");
			}

			var transactionRecord = _transactionService.GetTransactionById(dto.TransactionId);

            if (transactionRecord == null)
			{
				return BadRequest("Transaction record's not found!");
			}

			if (dto.CampaignType == CampaignType.Recommend)
			{

				if (dto.BookId == null) return BadRequest("BookId cannot be null!");
				if (_bookService.GetBookById((Guid)dto.BookId) == null) return BadRequest("Book doesn't existed!");

				var total = PPC_Price + subFee + numberOfTargetUser * costPerTargetUser;
				if(transactionRecord.Amount != total)
				{
					return BadRequest("The amount in transaction record doesn't met the total of model!");
				}
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
				{
					try
					{
						await _unitOfWork.AdsService.AddNewAd(new Advertisement
						{
							AdId = adId,
							AgencyId = dto.AgencyId,
							BookId = dto.BookId,
							Duration = dto.Duration,
							StartDate = DateTime.Now,
							EndDate = DateTime.Now.AddDays(duration),
							PPC_Price = PPC_Price,
							SubFee = subFee,
							TargetUserFee = numberOfTargetUser * costPerTargetUser,
							CampaignType = dto.CampaignType,
							TransactionId = dto.TransactionId
						});
						int changes = await _unitOfWork.Save();
						await transaction.CommitAsync();
						return (changes > 0) ? Ok(adId) : Ok("No changes!");
					}
					catch (Exception e)
					{
						await transaction.RollbackAsync();
						return BadRequest("An error occurred while adding new ad! Exception: " + e.Message);
					}
				}
			}

			if (dto.CampaignType == CampaignType.Display)
			{
                var total = displayBid;
                if (transactionRecord.Amount != total)
                {
                    return BadRequest("The amount in transaction record doesn't met the total of model!");
                }

                if (dto.BannerImg == null)
				{
					return BadRequest("Please upload an image of your banner!");
				}

                IFormFile? file = null;
                if (dto.BannerImg != null)
                {
                    byte[] fileBytes = Convert.FromBase64String(dto.BannerImg);

                    MemoryStream memoryStream = new MemoryStream(fileBytes);

                    file = new FormFile(memoryStream, 0, fileBytes.Length, null, "Siuuu")
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/octet-stream"
                    };
                }
                var saveProductResult = _cloudinaryService.UploadImage(file, $"Banners/{dto.AgencyId}/{adId}/Images");
                if (saveProductResult.StatusCode != 200 || saveProductResult.Data == null)
                {
                    return BadRequest(saveProductResult.Message);
                }

                var imageDir = saveProductResult.Data;

                using (var transaction = await _unitOfWork.BeginTransactionAsync())
				{
					try
					{
						Advertisement newAd = new Advertisement
						{
							AdId = adId,
							AgencyId = dto.AgencyId,
							BannerFee = displayBid,
							BannerDir = imageDir,
							Duration = "Week",
							StartDate = DateTime.Now,
							EndDate = DateTime.Now.AddDays(7),
							CampaignType = dto.CampaignType,
							TransactionId = dto.TransactionId
						};
						newAd.BookId = dto.BookId ?? null;
						await _unitOfWork.AdsService.AddNewAd(newAd);
						int changes = await _unitOfWork.Save();
						await transaction.CommitAsync();
						return (changes > 0) ? Ok(adId) : Ok("No changes!");
					}
					catch (Exception e)
					{
						await transaction.RollbackAsync();
						return BadRequest("An error occurred while adding new ad! Exception: " + e.Message);
					}
				}
			}
			return BadRequest("You must select a campaign!");
		}

		[HttpPost("checkout"), Authorize]
		public async Task<IActionResult> Checkout([FromBody] AdBillDTO dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("Model invalid!");
			}

			decimal costPerTargetUser = 1000;
			decimal subFee = 0;
			decimal total = 0;

			decimal PPC_Price = dto.PPC_Price ?? 0;
			decimal numberOfTargetUser = dto.NumberOfTargetUser ?? 0;
			decimal displayBid = dto.DisplayBid ?? 0;

			switch (dto.Duration)
			{
				case "Week":
					subFee = 200000;
					break;
				case "Month":
					subFee = 700000;
					break;
				case "Year":
					subFee = 8400000;
					break;
			}

			if (dto.CampaignType == CampaignType.Recommend)
			{
                if (dto.BookId == null) return BadRequest("BookId cannot be null!");
                if (_bookService.GetBookById((Guid)dto.BookId) == null) return BadRequest("Book doesn't existed!");
                total = subFee + numberOfTargetUser * costPerTargetUser + PPC_Price;
			}
			if (dto.CampaignType == CampaignType.Display)
			{
				if(dto.BookId != null) {
                    if (_bookService.GetBookById((Guid)dto.BookId) == null) return BadRequest("Book doesn't existed!");
                }
				total = displayBid;
			}

			int truncatedAmount = (int)Math.Round(total, MidpointRounding.AwayFromZero);

			NewTransactionDTO newTransDTO = new NewTransactionDTO()
			{
				PaymentContent = "Ecommerce ads checkout" + Guid.NewGuid(),
				PaymentCurrency = "vnd",
				RequiredAmount = truncatedAmount,
				ReferenceId = dto.TransactionId.ToString()
			};

			using (HttpClient client = new HttpClient())
			{
				var json = Newtonsoft.Json.JsonConvert.SerializeObject(newTransDTO);

				var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

				HttpResponseMessage response = await client.PostAsync("https://localhost:7138/api/Payment/vnpay/create-vnpay-link", content);
				;
				if (response.IsSuccessStatusCode)
				{
					string? responseJson = await response.Content.ReadAsStringAsync();

					PaymentLinkDTO? link = JsonConvert.DeserializeObject<PaymentLinkDTO>(responseJson);

					return Ok(link?.PaymentUrl);
				}
				else
				{
					return BadRequest("API request failed with status code: " + response.StatusCode);
				}
			}
		}

        [HttpGet("get-top-banners"), Authorize]
		public async Task<IActionResult> GetTopBanners()
        {
            var results = new List<TopBannerDTO>() {
                new TopBannerDTO
                {
                   AdId = Guid.Parse("b8beb15b-1d21-47fe-b69b-1c0e2a387292"),
                   Price = 100000,
                   BannerTitle = "Book ad: kill the mocking bird",
                   BannerDir = "https://res.cloudinary.com/dbpvdxzvi/image/upload/v1713452686/Banners/9e9ce971-0e33-4d03-a1d2-f1e65d948178/b8beb15b-1d21-47fe-b69b-1c0e2a387292/Images/oqo33zucdq22vlbrktoi.jpg",
                   ProductId = Guid.Parse("cdf2e3d4-5c6b-7a8b-9c0d-1e2f3e4d5c6b"),
                   AgencyId = Guid.Parse("9e9ce971-0e33-4d03-a1d2-f1e65d948178"),
                },
                new TopBannerDTO
                {
                   AdId = Guid.Parse("c06e42e7-27d2-4d4b-b1b3-aa140e7fe241"),
                   Price = 80000,
                   BannerTitle = "Book ad: greate gasby",
                   BannerDir = "https://res.cloudinary.com/dbpvdxzvi/image/upload/v1713452789/Banners/9e9ce971-0e33-4d03-a1d2-f1e65d948178/c06e42e7-27d2-4d4b-b1b3-aa140e7fe241/Images/uuosjebq17mevoekkngf.jpg",
                   ProductId = Guid.Parse("d3a5d8b7-4b2c-1a1a-9c8d-7e6f5a4b3c2d"),
                   AgencyId = Guid.Parse("9e9ce971-0e33-4d03-a1d2-f1e65d948178"),
                },
                new TopBannerDTO
                {
                   AdId = Guid.Parse("b0d4e8e9-4e9d-48a8-b3c9-2da1822cc893"),
                   Price = 60000,
                   BannerTitle = "Book ad: Harry Potter",
                   BannerDir = "https://res.cloudinary.com/dbpvdxzvi/image/upload/v1713397601/Banners/9e9ce971-0e33-4d03-a1d2-f1e65d948178/b0d4e8e9-4e9d-48a8-b3c9-2da1822cc893/Images/ugaktzb9fex2fivtk94k.jpg",
                   AgencyId = Guid.Parse("9e9ce971-0e33-4d03-a1d2-f1e65d948178"),
                },
            };
            return Ok(results);
        }

        [HttpGet("get-relevant-books"), Authorize]
		public async Task<IActionResult> GetRelevantBook()
		{
			var result = new List<RelevantBookDTO>()
			{
				new RelevantBookDTO
				{
					BookId = Guid.Parse("cdf2e3d4-5c6b-7a8b-9c0d-1e2f3e4d5c6b"),
					ImageDir = "https://res.cloudinary.com/dbpvdxzvi/image/upload/v1713452686/Banners/9e9ce971-0e33-4d03-a1d2-f1e65d948178/b8beb15b-1d21-47fe-b69b-1c0e2a387292/Images/oqo33zucdq22vlbrktoi.jpg",
					Price = 20000,
					Rating = 4.5,
					Title = "To Kill a Mockingbird"
                },
                new RelevantBookDTO
                {
                    BookId = Guid.Parse("d3a5d8b7-4b2c-1a1a-9c8d-7e6f5a4b3c2d"),
                    ImageDir = "https://res.cloudinary.com/dbpvdxzvi/image/upload/v1713452789/Banners/9e9ce971-0e33-4d03-a1d2-f1e65d948178/c06e42e7-27d2-4d4b-b1b3-aa140e7fe241/Images/uuosjebq17mevoekkngf.jpg",
                    Price = 18000,
                    Rating = 3.6,
                    Title = "The Great Gatsby"
                },
                new RelevantBookDTO
                {
                    BookId = Guid.Parse("abbc3f1d-ef8e-4d8a-9e8f-54321bcde987"),
                    ImageDir = "https://res.cloudinary.com/dbpvdxzvi/image/upload/v1713447579/Banners/9e9ce971-0e33-4d03-a1d2-f1e65d948178/a9ee3316-54cf-4165-aa6b-abfe47aba771/Images/wldzflivamwlaeuqm13d.jpg",
                    Price = 15000,
                    Rating = 4,
                    Title = "Harry Potter"
                }
            };
			return Ok(result);
		}
	}
}


using System.Security.Claims;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Rating;
using BusinessObjects.Models.Ecom.Rating;
using BusinessObjects.Models.Utils;
using DataAccess.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController: ControllerBase
	{
		private readonly IAccountService _accService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IRatingService _ratingService;
        private readonly IAddressService _addressService;
        private readonly IMapper _mapper;

        public AccountController(IMapper mapper,IAddressService addressService, IAccountService service, ICloudinaryService cloudinaryService, IRatingService ratingService)
        {
            _cloudinaryService = cloudinaryService;
            _ratingService = ratingService;
            _accService = service;
            _addressService = addressService;
            _mapper = mapper;
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] RegisterDTO model)
        {
            var status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please pass all the required fields";
                return BadRequest(status);
            }
            // check if users exists
            var userExists = await _accService.FindUserByEmailAsync(model.Email);
            if (userExists != null && userExists?.Username != null)
            {
                status.StatusCode = 0;
                status.Message = userExists.Username;
                return Ok(status);
            }
            AppUser user = await _accService.Register(model);
            await _ratingService.AddNewUserRatingAsync(user.UserId);
            return Ok(user);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] LoginDTO model)
        {
            var status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please pass all the required fields";
                return Ok(status);
            }
            AppUser? user = await _accService.FindUserByEmailAsync(model.Email);
            if (user == null)
            {
              return Unauthorized("User not found!");
            }
       
            if (!user.IsBanned)
            {
                byte[] salt = Convert.FromHexString(user.Salt);
                if (!_accService.VerifyPassword(model.Password, user.Password, salt))
                {
                    return Unauthorized("Wrong password");
                }
                string accessToken = await _accService.CreateTokenAsync(user);
                var refreshToken = await _accService.GenerateRefreshTokenAsync(user.UserId);

                if (refreshToken == null) return BadRequest("Fail to create refresh token");

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = refreshToken.ExpiredDate,
                    Secure = true 
                };
                Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

                return Ok(accessToken);
            }
            return BadRequest("Account is banned!");
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshTokenRequest)
        {
            var status = new Status();

            if (!ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please pass all the required fields";
                return BadRequest(status);
            }

            var refreshToken = await _accService.ValidateRefreshTokenAsync(refreshTokenRequest);

            if (refreshToken == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            if (refreshToken.ExpiredDate < DateTime.Now)
            {
                return Unauthorized("Refresh token has expired");
            }

            var user = await _accService.FindUserByIdAsync(refreshToken.UserId);

            if (user == null)
            {
                return Unauthorized("Invalid user");
            }

            var accessToken = await _accService.CreateTokenAsync(user);

            var newRefreshToken = await _accService.GenerateRefreshTokenAsync(user.UserId);

            if (newRefreshToken == null) return BadRequest("Fail to generate refresh token!");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.ExpiredDate,
                Secure = true 
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            return Ok(new { AccessToken = accessToken });
        }

        [HttpGet]
        [Route("get-default-address")]
        public IActionResult GetDefaultAddress(Guid userId)
        {
            try
            {
                Address? address = _accService.GetDefaultAddress(userId);
                if (address != null)
                {
                    return Ok(address);
                }
                else return BadRequest("Default Address' not set!!!");
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet, Authorize]
        [Route("get-user-profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                var roleClaim1 = HttpContext.User.FindFirst(ClaimTypes.Role);
                var usernameClaim = HttpContext.User.FindFirst(ClaimTypes.Name);
                var emailClaim = HttpContext.User.FindFirst(ClaimTypes.Email);
                List<string> roles = new List<string>();

                var roleClaims = HttpContext.User.FindAll(ClaimTypes.Role);

                if (userIdClaim != null)
                {
                    var userId = Guid.Parse(userIdClaim.Value);
                    if (roleClaims.Count() > 0)
                    {
                        foreach (var r in roleClaims)
                        {
                            roles.Add(r.Value);
                        }
                        if (usernameClaim != null)
                        {
                            if (emailClaim != null)
                            {
                                Address? address = _accService.GetDefaultAddress(userId);
                                string rendez = string.Empty;
                                if (address != null && address.Rendezvous != null)
                                {
                                    rendez = address.Rendezvous;
                                }


                                UserProfileDTO profile = new UserProfileDTO()
                                {
                                    UserId = userId,
                                    Username = usernameClaim.Value,
                                    Roles = roles,
                                    Address = rendez,
                                    Email = emailClaim.Value,
                                    IsValidated = await _accService.IsUserValidated(userId),
                                    //IsSeller = _accService.IsSeller(userId),
                                    IsBanned = await _accService.IsBanned(userId),
                                    Agencies = _accService.GetOwnerAgencies(userId)
                                };
                                return Ok(profile);
                            }
                            else return NotFound("Email claim not found!");
                        }
                        else return NotFound("Username claim not found!!!");
                    }
                    else return NotFound("Role claim not found!!!");
                }
                else return NotFound("User ID claim not found!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPut("set-is-account-validated")]
        public async Task<IActionResult> SetIsAccountValid([FromBody] UserValidationDTO dto)
        {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model invalid!");
                }

                var data = new NIC_Data
                {
                    Id = dto.NicId,
                    Fullname = dto.NicName,
                    Home = dto.NicHome,
                    Sex = dto.NicSex,
                    Nationality = dto.NicNationality
                };

                data.NICId = Guid.NewGuid();
                data.DateOfBirth = DateTime.Parse(dto.NicDoe);
                data.DateOfExpired = DateTime.Parse(dto.NicDoe);
                data.OwnerId = dto.UserId;

                int changes = await _accService.AddNicData(data);
                if(changes > 0)
                {
                    await _accService.SetUserIsValidated(true, dto.UserId);
                    return Ok("Successful!");
                } return BadRequest("Somthing went wrong");

            //return (changes > 0) ? Ok("Successful!") : BadRequest("Fail to save NIC data");

                //int result = await _accService.SetUserIsValidated(true, dto.);
                //IActionResult apiResult = (result > 0) ? Ok("Successful!") : BadRequest("No change!");
                //return apiResult;
        }

        [HttpPost("register-agency"), Authorize]
        public async Task<IActionResult> RegisterAgency([FromForm] AgencyRegistrationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model invalid!");
                }
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");

                if (userIdClaim == null)
                {
                    return BadRequest("User's not authenticated!");
                }
                Guid userId = Guid.Parse(userIdClaim.Value);
                var ownerId = userId;

                if(  await _addressService.GetAddressByIdAsync(dto.AddressId) == null)
                {
                    return BadRequest("AddressId's not valid!");
                }

                string logoUrl = "";

                if (dto.LogoImg != null)
                {
                    CloudinaryResponseDTO cldRspDTO = _cloudinaryService.UploadImage(dto.LogoImg, "Agencies/" + dto.AgencyName + "/Logo");
                    logoUrl = (cldRspDTO.StatusCode == 200 && cldRspDTO.Data != null)
                  ? cldRspDTO.Data : "";
                }

                if (await _accService.IsUserValidated(userId))
                {
                    string result = await _accService.RegisterAgency(dto, logoUrl, ownerId);
                    if (result == "Successful!")
                    {
                        return Ok(result);
                    }
                    else return BadRequest(result);
                }
                else return BadRequest("Account's not validated!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("get-agency-by-id")]
        public IActionResult GetAgencyById(Guid agencyId)
        {
            try
            {
                Agency result = _accService.GetAgencyById(agencyId);
                IActionResult response = (result.AgencyName != null) ? Ok(result) : NotFound("Agency doesn't exist!");
                return response;
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPut("update-agency")]
        public IActionResult UpdateAgency([FromForm] AgencyUpdateDTO dto)
        {
            string? logoUrl = null;

            if (dto.LogoImg != null)
            {
                CloudinaryResponseDTO cldRspDTO = _cloudinaryService.UploadImage(dto.LogoImg, "Agencies/" + dto.AgencyName + "/Logo");
                logoUrl = (cldRspDTO.StatusCode == 200 && cldRspDTO.Data != null)
              ? cldRspDTO.Data : null;
            }

            int changes = _accService.UpdateAgency(dto, logoUrl);
            IActionResult response = (changes > 0) ? Ok("Successful!") : Ok("No changes");
            return response;
        }

        // ------------------------------------------- DATDQ -----------------------------------------\\
        [HttpGet, Authorize]
        [Route("Get-Agency-Analyst")]
        public IActionResult GetAgencyAnalyst()
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }
                return Ok(_accService.GetAgencyAnalyst(agencyId));
            }
            catch (Exception e)
            {
                return BadRequest("Can't get Agency Analyst");
            }
        }

        [HttpGet, Authorize]
        [Route("Get-Agency-Analyst-By-Time")]
        public IActionResult GetAgencyAnalystByTime(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > DateTime.Today.Date && endDate > DateTime.Today.Date)
                {
                    return BadRequest("Date input invalid");
                }
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }
                return Ok(_accService.GetAgencyAnalystByTime(agencyId, startDate, endDate));
            }
            catch (Exception e)
            {
                return BadRequest("Can't get Agency Analyst");
            }
        }

        // ------------------------------------------- END DATDQ -----------------------------------------\\

        /*-------------------------------------------SONDB-----------------------------------------*/

        /*-------------------------------------------END SONDB-----------------------------------------*/
        [HttpPut("update-user-profile")] //SONDB 
        public async Task<IActionResult> UpdateUserProfile([FromForm] UserProfile formData)
        {
            try
            {
               
                // Extract data from the form data object
                string? username = await _accService.GetUsernameById(formData.UserId);
                string newImgPath = "";

                if (ModelState.IsValid)
                {
                    if (formData.AvatarDir != null)
                    {
                        var user = await _accService.GetUserById(formData.UserId);
                        if (user != null)
                        {
                            string? oldImgPath = user.AvatarDir;

                            if (oldImgPath != null)
                            {
                                _cloudinaryService.DeleteImage(oldImgPath, "Profile");
                            }
                            var cloudResponse = _cloudinaryService.UploadImage(formData.AvatarDir, "UserProfile/" + formData.Username + "/Image");
                            if (cloudResponse.StatusCode != 200)
                            {
                                return BadRequest(cloudResponse.Message);
                            }
                            newImgPath = cloudResponse.Data;
                        }
                    }

                    // Create the updated user object
                    var updateData = new AppUser
                    {
                        UserId = formData.UserId,
                        Username = formData.Username,
                        AvatarDir = newImgPath,
                        Email = formData.Email,
                        Phone = formData.Phone,
                    };

                    // Call the service method to update the profile
                    var result = await _accService.UpdateProfile(updateData);

                    // Check the result of the update operation
                    if (result > 0)
                    {
                        return Ok("Successful");
                    }
                    else
                    {
                        return BadRequest("Update failed please recheck");
                    }
                }
                else
                {
                    return BadRequest("Model state invalid");
                }
            }
            catch (Exception e)
            {
                // Log the exception
                string errorMessage = $"An error occurred while updating the user profile: {e.Message}. Inner exception: {e.InnerException?.Message}. Stack trace: {e.StackTrace}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAllUser([FromQuery] PagingParams @params)
        {
            try
            {
                var posts =await _accService.GetAllUsers(@params);


                if (posts != null)
                {
                    var metadata = new
                    {
                        posts.TotalCount,
                        posts.PageSize,
                        posts.CurrentPage,
                        posts.TotalPages,
                        posts.HasNext,
                        posts.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(posts);
                }
                else return BadRequest("No user!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            try
            {
                var user = await _accService.GetUserById(userId);
                return Ok(user);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPost("rate-and-comment")] //SONDB 
        public async Task<IActionResult> RateAndCommentProduct([FromForm] RatingRecordDTO ratingRecordDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model invalid");
                }

                using (var context = new AppDbContext())
                {
                    if (int.TryParse(ratingRecordDTO.RatingPoint, out int ratingPoint))
                    {
                        // Check if the user has already rated the product
                        var existingRatingRecord = context.RatingRecords
                            .FirstOrDefault(record => record.ReviewerId == ratingRecordDTO.UserId && record.RatingId == ratingRecordDTO.RatingId);

                        if (existingRatingRecord != null)
                        {
                            return BadRequest("You have already rated and commented on this product.");
                        }
                        else
                        {
                            // Create a new rating record
                            var newRatingRecord = new RatingRecord
                            {
                                RatingRecordId = Guid.NewGuid(),
                                RatingId = ratingRecordDTO.RatingId,
                                ReviewerId = ratingRecordDTO.UserId,
                                RatingPoint = ratingPoint,
                                Comment = ratingRecordDTO.Comment,
                                CreatedDate = DateTime.Now,

                            };

                            await _ratingService.RateAndCommentAsync(newRatingRecord);
                        }

                        // Save changes to the database
                        await context.SaveChangesAsync();
                        var userPublicProfile = await _accService.GetUserById(ratingRecordDTO.UserId);

                        // Construct the response object
                        var response = new RatingResponseDTO
                        {
                            RatingRecordId = ratingRecordDTO.RatingRecordId,
                            RatingId = ratingRecordDTO.RatingId,
                            ReviewerId = ratingRecordDTO.UserId,
                            RatingPoint = ratingPoint,
                            Comment = ratingRecordDTO.Comment,
                            Replies = new List<object>(), // Assuming there are no replies initially
                            CreatedDate = DateTime.Now,
                            Email = userPublicProfile.Email,
                            Username = userPublicProfile.Username,
                            AvatarDir = userPublicProfile.AvatarDir
                        };

                        var ratingsForRatingId = context.RatingRecords.Where(record => record.RatingId == ratingRecordDTO.RatingId).ToList();
                        double totalRatingPoints = ratingsForRatingId.Sum(record => record.RatingPoint);
                        double averageRating = ratingsForRatingId.Count > 0 ? totalRatingPoints / ratingsForRatingId.Count : 0;

                        // Update the OverallRating property in the Rating entity
                        var ratingEntity = context.Ratings.FirstOrDefault(r => r.RatingId == ratingRecordDTO.RatingId);
                        if (ratingEntity != null)
                        {
                            ratingEntity.OverallRating = averageRating;
                        }
                        else
                        {
                            // If RatingId not found, create a new Rating entity
                            ratingEntity = new Rating { RatingId = ratingRecordDTO.RatingId, OverallRating = averageRating };
                            context.Ratings.Add(ratingEntity);
                        }

                        // Save changes to the database
                        context.SaveChanges();

                        // Return a success response
                        return Ok(response);
                    }
                    else
                    {
                        // Return bad request if the ratingPoint string cannot be parsed to int
                        return BadRequest("Invalid rating point format");
                    }
                }
            }
            catch (Exception ex)
            {
                // Return the error response if an exception occurs
                return StatusCode(500, $"An error occurred while retrieving reviews for agency ID  Error Details: {ex.Message}. Stack Trace: {ex.StackTrace}");
            }
        }

        [HttpGet("get-book-review-by-bookid")] //SONDB
        public async Task<IActionResult> GetCommentsByUserId(Guid bookId, [FromQuery] PagingParams @params) //SONDB
        {
            try
            {
                var commentbook = await _ratingService.GetCommentsByBookId(bookId, @params);

                if (commentbook != null)
                {
                    var commentsWithUsers = new List<CommentWithUserDTO>();

                    foreach (var comment in commentbook)
                    {
                        var user = await _accService.GetUserById(comment.ReviewerId);
                        if (user != null)
                        {
                            var commentWithUser = new CommentWithUserDTO
                            {
                                RatingRecordId = comment.RatingRecordId,
                                RatingId = comment.RatingId,
                                ReviewerId = comment.ReviewerId,
                                RatingPoint = comment.RatingPoint,
                                Comment = comment.Comment,
                                CreatedDate = comment.CreatedDate,
                                Email = user.Email,
                                Username = user.Username,
                                AvatarDir = user.AvatarDir,
                                Replies = new CommentReplyDTO(), // Initialize replies list
                            };

                            // Fetch replies for the current comment
                            var replies = await _ratingService.GetRepliesForRatingRecordId(comment.RatingRecordId);
                            foreach (var reply in replies)
                            {
                                // Fetch agency information based on AgencyId
                                var agency = await _accService.GetAgencyByIdAsync(reply.AgencyId);
                                if (agency != null)
                                {
                                    // Check if the agency has the book in its inventory

                                    var replyDTO = new CommentReplyDTO
                                    {
                                        ReplyId = reply.ReplyId,
                                        ReplyText = reply.ReplyText,
                                        AgencyId = reply.AgencyId,
                                        AgencyName = agency.AgencyName,
                                        LogoUrl = agency.LogoUrl,
                                        CreatedDate = reply.CreatedDate
                                    };
                                    commentWithUser.Replies = replyDTO;

                                }
                            }

                            commentsWithUsers.Add(commentWithUser);
                        }
                    }

                    // Construct the response object including comments, associated users, and pagination metadata
                    var metadata = new
                    {
                        commentbook.TotalCount,
                        commentbook.PageSize,
                        commentbook.CurrentPage,
                        commentbook.TotalPages,
                        commentbook.HasNext,
                        commentbook.HasPrevious
                    };

                    // Serialize the response and add pagination headers
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(commentsWithUsers);
                }
                else
                {
                    return BadRequest("No review found for the specified book");
                }
            }
            catch (Exception e)
            {
                // Log the error message along with the stack trace
                string errorMessage = $"An error occurred while retrieving comments: {e.Message}. Stack Trace: {e.StackTrace}";
                Console.WriteLine(errorMessage);

                // Return the error message along with status code 500
                return StatusCode(500, errorMessage);
            }
        }

        [HttpPost("add-reply")] //SONDB 
        public async Task<IActionResult> AddReplyAsync([FromBody] ReplyAddDTO reply)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int result = await _ratingService.AddNewReplyAsync(new Reply()
                    {
                        ReplyId = Guid.NewGuid(),
                        RatingRecordId = reply.RatingRecordId,
                        AgencyId = reply.AgencyId,
                        ReplyText = reply.ReplyText,
                        CreatedDate = DateTime.Now,
                        HasReplied = true,
                    });
                    if (result > 0)
                    {
                        return Ok();
                    }
                    return BadRequest("Add false");
                }
                return BadRequest("Reply Invalid");
            }
            catch (Exception ex)
            {
                // Log the exception and its inner exception
                Console.WriteLine("Error occurred while adding a reply:");
                Console.WriteLine("Inner Exception Message: " + ex.InnerException?.Message);
                Console.WriteLine("Stack Trace: " + ex.InnerException?.StackTrace);

                // Rethrow the exception or handle it appropriately
                throw new Exception("An error occurred while adding a reply.", ex);
            }
        }

        [HttpPost("update-reply")] //SONDB 
        public async Task<IActionResult> UpdateReplyAsync([FromBody] ReplyUpdateDTO reply)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int result = await _ratingService.UpdateReplyAsync(new Reply()
                    {
                        ReplyId = reply.ReplyId,
                        RatingRecordId = reply.RatingRecordId,
                        AgencyId = reply.AgencyId,
                        ReplyText = reply.ReplyText,
                        CreatedDate = DateTime.Now
                    });
                    if (result > 0)
                    {
                        return Ok();
                    }
                    return BadRequest("Update false");
                }
                return BadRequest("Reply Invalid");
            }
            catch (Exception ex)
            {
                // Log the exception and its inner exception
                Console.WriteLine("Error occurred while adding a reply:");
                Console.WriteLine("Inner Exception Message: " + ex.InnerException?.Message);
                Console.WriteLine("Stack Trace: " + ex.InnerException?.StackTrace);

                // Rethrow the exception or handle it appropriately
                throw new Exception("An error occurred while adding a reply.", ex);
            }
        }

        [HttpDelete("delete-reply")] //SONDB 
        public async Task<IActionResult> DeleteReplyByIdAsync(Guid replyid)
        {
            try
            {
                await _ratingService.DeleteReplyByIdAsync(replyid);
                var response = new
                {
                    StatusCode = 204,
                    Message = "Delete reply query was successful",
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 500,
                    Message = "Delete comment query Internal Server Error",
                    Error = ex,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("percentage-reply-by-agency")] //SONDB 
        public async Task<IActionResult> GetReplyPercentagesForAgency(Guid agencyId)
        {
            try
            {
                var replyPercentages = await _accService.CalculateOverallReplyPercentageForAgencyAsync(agencyId);
                return Ok(replyPercentages);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet, Authorize]
        [Route("get-review-for-agency")] //SONDB
        public async Task<IActionResult> GetReviewForAgency([FromQuery] ReviewSearchCriteria searchCriteria, [FromQuery] PagingParams @params)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = await _accService.GetAgencyIdAsync(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id not found for the given owner");
                }

                var reviews = await _ratingService.GetReviewsByAgencyId(agencyId, searchCriteria, @params);
                var commentWithUsers = new List<ReviewDTO>();

                foreach (var comment in reviews)
                {
                    var user = await _accService.GetUserById(comment.ReviewerId);
                    var ratebook = await _ratingService.GetBookByRatingId(comment.RatingId ?? Guid.Empty);
                    if (user != null)
                    {
                        var commentWithUser = new ReviewDTO
                        {
                            RatingRecordId = comment.RatingRecordId,
                            RatingId = comment.RatingId ?? Guid.Empty,
                            BookId = ratebook.ProductId,
                            BookName = ratebook.Name,
                            UserId = comment.ReviewerId,
                            RatingPoint = comment.RatingPoint,
                            Comment = comment.Comment,
                            CreatedDate = comment.CreatedDate,
                            Email = user.Email,
                            Username = user.Username,
                            AvatarDir = user.AvatarDir,
                            Reply = new CommentReplyDTO(),
                        };

                        // Fetch replies for the current comment
                        var replies = await _ratingService.GetRepliesForRatingRecordId(comment.RatingRecordId);
                        foreach (var reply in replies)
                        {
                            var agency = await _accService.GetAgencyByIdAsync(reply.AgencyId);
                            if (agency != null)
                            {
                                var replyDTO = new CommentReplyDTO
                                {
                                    ReplyId = reply.ReplyId,
                                    ReplyText = reply.ReplyText,
                                    AgencyId = reply.AgencyId,
                                    AgencyName = agency.AgencyName,
                                    LogoUrl = agency.LogoUrl,
                                    CreatedDate = reply.CreatedDate
                                };
                                commentWithUser.Reply = replyDTO;
                            }
                        }

                        commentWithUsers.Add(commentWithUser);
                    }
                }
                var metadata = new
                {
                    reviews.TotalCount,
                    reviews.PageSize,
                    reviews.CurrentPage,
                    reviews.TotalPages,
                    reviews.HasNext,
                    reviews.HasPrevious
                };

                // Serialize the response and add pagination headers
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                return Ok(commentWithUsers);

            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred while retrieving reviews for agency ID . Error Details: {e.Message}. Stack Trace: {e.StackTrace}");
            }
        }

        [HttpPost("SendEmailVerification")]
        public async Task<IActionResult> SendEmailVerification([FromBody] VerificationRequest request)
        {
            try
            {
                await _accService.SendVerificationEmail(request.Email, request.UserId);
                return Ok("Notification email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send verification email: {ex.Message}");
            }
        }
        //[HttpGet("verify-email/{userId}")]
        //public async Task<IActionResult> VerifyEmail(Guid userId)
        //{
        //    var success = await _accService.VerifyEmail(userId);

        //    if (success)
        //    {
        //        return Ok("Email verified successfully");
        //    }
        //    else
        //    {
        //        return NotFound("User not found");
        //    }
        //}

    }
}


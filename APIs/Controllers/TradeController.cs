using System.Security.Claims;
using APIs.Services;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using AutoMapper;
using BusinessObjects.DTO;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Ecom.Rating;
using BusinessObjects.Models.Trading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIs.Controllers
{
    [Route("api/trading")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IAccountService _accountService;
        private readonly IAddressService _addressService;
        private readonly ITradeService _tradeService;
        private readonly IRatingService _ratingService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public TradeController(IMapper mapper, IUnitOfWork unitOfWork, IPostService postService, ICloudinaryService cloudinaryService, IAccountService accountService,
            IAddressService addressService, ITradeService tradeService, IRatingService ratingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _ratingService = ratingService;
            _cloudinaryService = cloudinaryService;
            _addressService = addressService;
            _postService = postService;
            _accountService = accountService;
            _tradeService = tradeService;
        }

        [HttpGet("get-post-interested-by-user"), Authorize]
        public async Task<IActionResult> GetPostInteredtedByUserIdAsync([FromQuery] PagingParams @params)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);
            var posts = await _postService.GetPostInterestedInByUserId(userId, @params);

            var result = await _postService.ConvertToPostDetailsListAsync(posts);

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
                return Ok(result);
            }
            else return Ok("No post found!");
        }

        //---------------------------------------------POSTINTEREST-------------------------------------------------------//

        [HttpGet("get-post-interest-by-post-id")]
        public async Task<IActionResult> GetPostInterestByPostIdAsync(Guid postId, [FromQuery] PagingParams @params)
        {
            try
            {
                var postInteresters = await _postService.GetInteresterByPostIdAsync(postId, @params);
                List<InteresterDetailsDTO> result = new List<InteresterDetailsDTO>();

                foreach (var p in postInteresters)
                {
                    var user = await _accountService.FindUserByIdAsync(p.InteresterId);
                    if (user != null)
                    {
                        result.Add(new InteresterDetailsDTO
                        {
                            RecordId = p.PostInterestId,
                            UserId = p.InteresterId,
                            AvatarDir = user.AvatarDir,
                            Username = user.Username,
                            CreateDate = p.CreateDate,
                            IsChosen = p.IsChosen
                        });
                    }
                }
                if (postInteresters != null)
                {
                    var metadata = new
                    {
                        postInteresters.TotalCount,
                        postInteresters.PageSize,
                        postInteresters.CurrentPage,
                        postInteresters.TotalPages,
                        postInteresters.HasNext,
                        postInteresters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(result);
                }
                else return Ok(postInteresters);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("get-traderId-by-postId")]
        public async Task<IActionResult> GetTraderIdsByPostId(Guid postId)
        {
            try
            {
                var result = await _tradeService.GetTraderIdsByPostIdAsync(postId);
                GetTraderIdsByPostIdDTO response = new GetTraderIdsByPostIdDTO
                {
                    OwnerId = result.SingleOrDefault().OwnerId,
                    InteresterId = result.SingleOrDefault().InteresterId
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 500,
                    Message = "Fail to get traderIds! Internal Server Error",
                    Error = ex,
                };
                return StatusCode(500, response);
            }

        }

        [HttpPost("add-post-interester"), Authorize]
        public async Task<IActionResult> AddNewPostInterestAsync([FromForm] AddPostInterestDTO postInterest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid comment!");
                }
                var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("User is not authenticated");
                }
                var userId = Guid.Parse(userIdClaim.Value);
                if (!await _postService.IsTradePostAsync(postInterest.PostId))
                {
                    return BadRequest("Not a trade post!");
                }

                if (await _postService.IsPostOwnerAsync(postInterest.PostId, userId))
                {
                    return BadRequest("Owners cannot express interest in their own post!");
                }

                if (await _postService.IsLockedPostAsync(postInterest.PostId))
                {
                    return BadRequest("Post is locked!");
                }

                if (await _postService.IsAlreadyInterestedAsync(userId, postInterest.PostId))
                {
                    return BadRequest("Already interested in this post!");
                }

                int result = await _postService.AddNewInteresterAsync(new PostInterester()
                {
                    PostInterestId = Guid.NewGuid(),
                    PostId = postInterest.PostId,
                    InteresterId = userId,
                    CreateDate = DateTime.Now
                });

                if (result > 0)
                {
                    return Ok("Successful!");
                }

                return BadRequest("Failed to add interest!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPost("accept-trade"), Authorize]
        public async Task<IActionResult> AccepTradeAsync([FromBody] AcceptTradeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            bool isPostOwner = await _postService.IsPostOwnerAsync(dto.PostId, Guid.Parse(userIdClaim.Value));
            if (!isPostOwner)
            {
                return BadRequest("User is not the post owner");
            }
            int lockResult = await _postService.SetLockPostAsync(true, dto.PostId);

            if (lockResult <= 0)
            {
                return BadRequest("Failed to lock post");
            }

            Guid? recordId = await _postService.GetLockedRecordIdAsync(dto.InteresterId, dto.PostId);
            if (recordId == null)
            {
                await _postService.SetLockPostAsync(false, dto.PostId);
                return BadRequest("Lock record not found");
            }

            int isChosenChanges = await _postService.SetIsChosen(true, (Guid)recordId);
            if (isChosenChanges <= 0)
            {
                await _postService.SetLockPostAsync(false, dto.PostId);
                return BadRequest("Failed to set chosen trader. Reverted lock post");
            }
            else
            {
                int tradeDetailsChanges = await _tradeService.AddNewTradeDetailsAsync(new TradeDetails
                {
                    TradeDetailId = Guid.NewGuid(),
                    LockedRecordId = (Guid)recordId,
                    Status = TradeStatus.NotSubmitted,
                    IsPostOwner = true
                });
                tradeDetailsChanges += await _tradeService.AddNewTradeDetailsAsync(new TradeDetails
                {
                    TradeDetailId = Guid.NewGuid(),
                    LockedRecordId = (Guid)recordId,
                    Status = TradeStatus.NotSubmitted,
                    IsPostOwner = false
                });
                if (tradeDetailsChanges <= 1)
                {
                    var respone = new
                    {
                        StatusCode = 500,
                        Message = "Fail to add trade details!"
                    };
                    return StatusCode(500, respone);
                }
            }
            return Ok(dto.PostId);
        }

        [HttpPut("submit-trade-details")]
        public async Task<IActionResult> SubmitTradeDetails([FromBody] SubmitTradeDetailDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model state is invalid!");
            }

            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = Guid.Parse(userIdClaim.Value);

            bool isPostOwner = await _postService.IsPostOwnerAsync(dto.PostId, userId);

            if (isPostOwner != dto.IsPostOwner)
            {
                return BadRequest("User cannot submitted the other trader form!");
            }

            Guid? oldAddress = await _addressService.CheckAddressExisted(new CompareAddressDTO
            {
                City_Province = dto.City_Province,
                District = dto.District,
                SubDistrict = dto.SubDistrict,
                Rendezvous = dto.Rendezvous,
                UserId = userId
            });

            Guid? addressId = null;
            int addressChanges = 0;

            if (oldAddress == null)
            {
                addressId = Guid.NewGuid();
                addressChanges += _addressService.AddNewAddress(new Address
                {
                    AddressId = (Guid)addressId,
                    City_Province = dto.City_Province,
                    District = dto.District,
                    SubDistrict = dto.SubDistrict,
                    Rendezvous = dto.Rendezvous,
                    UserId = userId
                });
            }

            Guid? lockedRecordId = await _postService.GetLockedRecordIdAsync(userId, dto.PostId);

            if (lockedRecordId == null)
            {
                if (addressChanges > 0 && addressId != null)
                {
                    await _addressService.DeleteAddressAsync((Guid)addressId);
                }
                return BadRequest("Locked record not found!");
            }

            TradeDetails details = new TradeDetails
            {
                TradeDetailId = dto.TradeDetailsId,
                IsPostOwner = isPostOwner,
                LockedRecordId = (Guid)lockedRecordId,
                Note = dto.Note,
                Phone = dto.Phone
            };

            if (addressChanges > 0)
            {
                details.AddressId = addressId;
            }
            else
            { details.AddressId = oldAddress; }

            int tradeDetailsChanges = await _tradeService.UpdateTradeDetails(details);

            if (tradeDetailsChanges > 0)
            {
                //return Ok($"Is old address null: {oldAddress == null}, Address changes: " + $"{addressChanges}, Updated address: " + $"{details.AddressId}");
                return Ok(details);
            }
            else return Ok("No changes!");

        }

        [HttpGet("get-locked-post-by-userId"), Authorize]
        public async Task<IActionResult> GetLockedPostsByUserId(TraderType traderType, [FromQuery] PagingParams @params)
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            try
            {
                var result = await _tradeService.GetLockedPostsByUserId(Guid.Parse(userIdClaim.Value), traderType, @params);
                List<GetLockedPostsByUserIdDTO> response = new List<GetLockedPostsByUserIdDTO>();
                foreach (var r in result)
                {
                    Post? post = await _postService.GetPostByIdAsync(r.PostId);
                    if (post != null)
                    {
                        GetLockedPostsByUserIdDTO temp = new GetLockedPostsByUserIdDTO
                        {
                            PostId = r.PostId,
                            Status = r.TradeStatus.ToString(),
                            Title = post.Title,
                            CreateDate = post.CreatedAt
                        };
                        response.Add(temp);
                    }
                }

                if (result != null)
                {
                    var metadata = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(response);
                }
                else return Ok(result);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 500,
                    Message = "Fail to get traderIds! Internal Server Error",
                    Error = ex,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("get-trade-details-by-postId"), Authorize]
        public async Task<IActionResult> GetTradeDetailsByPostId(Guid postId)
        {
            List<GetTradeDetailsByPostIdDTO> response = new List<GetTradeDetailsByPostIdDTO>();
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var roleClaims = HttpContext.User.FindAll(ClaimTypes.Role);

            if (roleClaims == null)
            {
                return BadRequest("No role claim found!");
            }
            var roles = new List<string>();

            foreach (var c in roleClaims)
            {
                roles.Add(c.Value);
            }

            var traderIds = await _tradeService.GetTraderIdsByPostIdAsync(postId);
            if (traderIds.Any(g => g.OwnerId != userId && g.InteresterId != userId) && !roles.Contains("Staff"))
            {
                return BadRequest("You don't have permission to do this!");
            }
            var tradeDetails = await _tradeService.GetTradeDetailsByPostIdAsync(postId);

            foreach (var td in tradeDetails)
            {
                Guid traderId = traderIds.SingleOrDefault().InteresterId;
                if (td.IsPostOwner)
                {
                    traderId = traderIds.SingleOrDefault().OwnerId;
                }
                response.Add(new GetTradeDetailsByPostIdDTO
                {
                    Details = td,
                    TraderId = traderId
                });
            }
            return Ok(response);
        }

        [HttpPut("set-trade-status"), Authorize]
        public async Task<IActionResult> SetTradeStatus([FromBody] TradeStatusUpdateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model state invalid!");
                }
                var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("User is not authenticated");
                }

                var userId = Guid.Parse(userIdClaim.Value);

                var roleClaims = HttpContext.User.FindAll(ClaimTypes.Role);

                if (roleClaims == null)
                {
                    return BadRequest("No role claim found!");
                }
                var roles = new List<string>();

                foreach (var c in roleClaims)
                {
                    roles.Add(c.Value);
                }

                bool isPostOwner = await _postService.IsPostOwnerAsync(dto.PostId, userId);

                TradeDetails? record = await _tradeService.GetTradeDetailsById(dto.TradeDetailsId);
                if (record == null)
                {
                    return Ok("Trade details doesn't existed!");
                }

                if (await _unitOfWork.BookCheckListService.IsCheckListExisted(dto.TradeDetailsId))
                {
                    return BadRequest("Checklist hasn't been added yet!");
                }

                if (dto.UpdatedStatus == TradeStatus.OnDeliveryToMiddle)
                {
                    if (isPostOwner == record.IsPostOwner)
                    {
                        return BadRequest("Users cannot change the status of their own form!");
                    };
                    if (await _unitOfWork.BookCheckListService.IsAnyInChecklistNotSubmitted(dto.TradeDetailsId, "Trader"))
                    {
                        return BadRequest("Some fields in the checklist have not been submitted!");
                    }
                }

                if (dto.UpdatedStatus == TradeStatus.Successful || dto.UpdatedStatus == TradeStatus.OnDeliveryToMiddle)
                {
                    if (isPostOwner == record.IsPostOwner)
                    {
                        return BadRequest("Users cannot change the status of their own form!");
                    }
                }

                if ((dto.UpdatedStatus == TradeStatus.MiddleReceived || dto.UpdatedStatus == TradeStatus.WaitFoeChecklistConfirm || dto.UpdatedStatus == TradeStatus.OnDevliveryToTrader) && !roles.Contains("Staff"))
                {
                    return BadRequest("You must be a system staff to do this!");
                }

                int changes = await _tradeService.SetTradeStatus(dto.UpdatedStatus, dto.TradeDetailsId);
                return (changes > 0) ? Ok("Successful!") : Ok("No changes!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpDelete("delete-post-interest")]
        public async Task<IActionResult> DeletePostInterestByIdAsync(Guid postInterestId)
        {
            try
            {
                await _postService.DeleteInteresterByIdAsync(postInterestId);
                var response = new
                {
                    StatusCode = 204,
                    Message = "Delete postInterest query was successful",
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 500,
                    Message = "Delete postInterest query Internal Server Error",
                    Error = ex,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost("rate-user-post-trade"), Authorize]
        public async Task<IActionResult> RateUserPostTrade([FromBody] PostTradeRatingDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model invalid");
                }
                var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("User is not authenticated");
                }
                var userId = Guid.Parse(userIdClaim.Value);
                if (userId == dto.RevieweeId)
                {
                    return BadRequest("User can rate their own result!");
                }
                Guid? revieweeRatingId = await _ratingService.GetRatingIdByRevieweeIdAsync(dto.RevieweeId);
                RatingRecord newRecord = new RatingRecord
                {
                    RatingRecordId = Guid.NewGuid(),
                    RatingId = revieweeRatingId,
                    RatingPoint = dto.RatingPoint,
                    Comment = dto.Comment,
                    ReviewerId = userId,
                    RatingType = RatingType.Trade
                };
                if (await _ratingService.AddNewUserRatingRecordAsync(newRecord) <= 0)
                {
                    return BadRequest("Fail to add new rating record!");
                }
                int ratingRecordUpdates = await _tradeService.UpdateRatingRecordIdAsync(newRecord.RatingRecordId, dto.TradeDetailsId, (Guid)newRecord.RatingId);
                return (ratingRecordUpdates > 0) ? Ok(newRecord) : Ok("Fail to add new rating record!");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    StatusCode = 500,
                    Message = "Fail to add new rating record! Internal Server Error",
                    Error = ex,
                };
                return StatusCode(500, response);
            }
        }

        //---------------------------------------------Middle-------------------------------------------------------//
        [HttpGet("get-middle-man-address"), Authorize]
        public IActionResult GetMiddleManAddress()
        {
            List<MiddleManAddressDTO> result = new List<MiddleManAddressDTO>();
            PagingParams @params = new PagingParams();
            var adresses = _addressService.GetAllUserAddress(Guid.Parse("24b54aec-dea5-4c35-980b-6b15813e0398"), @params);

            foreach (var a in adresses)
            {
                result.Add(new MiddleManAddressDTO
                {
                    AddressId = a.AddressId,
                    Address = a.City_Province + ", " + a.District + ", " + a.SubDistrict + ", " + a.Rendezvous
                });
            }
            return Ok(result);
        }

        [HttpGet("get-all-trade-post-for-middle"), Authorize]
        public async Task<IActionResult> GetAllTradePostForMiddle([FromQuery] PagingParams @params)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var roleClaims = HttpContext.User.FindAll(ClaimTypes.Role);

            if (roleClaims == null)
            {
                return BadRequest("No role claim found!");
            }
            var roles = new List<string>();

            foreach (var c in roleClaims)
            {
                roles.Add(c.Value);
            }

            if (!roles.Contains("Staff"))
            {
                return BadRequest("You must be a system staff to do this!");
            }

            return Ok(await _tradeService.GetAllTradePostForMiddle(@params));
        }
        //---------------------------------------------Book Check List-------------------------------------------------------//
        [HttpPost("add-book-check-list"), Authorize]
        public async Task<IActionResult> AddBookCheckList([FromBody] AddBookCheckListRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model invalid!");
            }

            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            if (await _tradeService.GetTradeDetailsById(dto.TradeDetailsId) == null)
            {
                return BadRequest("Trade details doesn't exist!");
            }

            if (await _tradeService.GetPostByTradeDetailsId(dto.TradeDetailsId) == null)
            {
                return BadRequest("Can't find the post to which the trade details belong!");
            }

            var userId = Guid.Parse(userIdClaim.Value);

            if (await _tradeService.IsTradeDetailsOwner(userId, dto.TradeDetailsId))
            {
                return BadRequest("Users cannot create checklists for their own books!");
            }

            if (await _unitOfWork.BookCheckListService.IsCheckListExisted(dto.TradeDetailsId))
            {
                return BadRequest("Trade details already have a checklist!");
            }

            List<BookCheckList> targets = new List<BookCheckList>()
            {
                new BookCheckList
                {
                    Id = Guid.NewGuid(),
                    Target = "Front Cover (Image)",
                    TradeDetailsId = dto.TradeDetailsId,
                },
                new BookCheckList
                {
                    Id = Guid.NewGuid(),
                    Target = "Spine (Image)",
                    TradeDetailsId = dto.TradeDetailsId
                },
                new BookCheckList
                {
                    Id = Guid.NewGuid(),
                    Target = "Back Cover (Image)",
                    TradeDetailsId = dto.TradeDetailsId
                },
                new BookCheckList
                {
                    Id = Guid.NewGuid(),
                    Target = "Pages (Video)",
                    TradeDetailsId = dto.TradeDetailsId
                }
            };

            if (dto.Targets != null)
            {
                if (dto.Targets.Count > 3)
                {
                    return BadRequest("Cannot add more than 3 targets!");
                }
                foreach (var t in dto.Targets)
                {
                    targets.Add(new BookCheckList
                    {
                        Id = Guid.NewGuid(),
                        Target = t,
                        TradeDetailsId = dto.TradeDetailsId
                    });
                }
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    await _unitOfWork.BookCheckListService.AddMultipleCheckList(targets);
                    int changes = await _unitOfWork.Save();
                    await transaction.CommitAsync();
                    return Ok(changes);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("An error occurred while adding check list! Exception: " + e.Message);
                }
            }
        }

        [HttpGet("get-check-list-by-trade-details-id"), Authorize]
        public async Task<IActionResult> GetCheckListByTradeDetailsId(Guid id)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var post = await _tradeService.GetPostByTradeDetailsId(id);

            if (post == null) { return BadRequest("Can't find the post to which the trade details belong!"); }

            var traderIds = await _tradeService.GetTraderIdsByPostIdAsync(post.PostId);
            if (traderIds.Any(g => g.OwnerId != userId && g.InteresterId != userId))
            {
                return BadRequest("User are not a trader of this transaction!");
            }

            var result = await _unitOfWork.BookCheckListService.GetCheckListByTradeDetailsId(id);

            return Ok(result);
        }

        [HttpPut("update-check-list"), Authorize]
        public async Task<IActionResult> UpdateCheckList([FromForm] CheckListUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model invalid!");
            }

            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var roleClaims = HttpContext.User.FindAll(ClaimTypes.Role);

            if (roleClaims == null)
            {
                return BadRequest("No role claim found!");
            }
            var roles = new List<string>();

            foreach (var c in roleClaims)
            {
                roles.Add(c.Value);
            }

            var record = await _unitOfWork.BookCheckListService.GetById(dto.Id);
            if (record == null)
            {
                return BadRequest("Checklist record not found!");
            }

            var updatedData = _mapper.Map<BookCheckList>(dto);

            if (dto.Target != null && dto.Target != record.Target)
            {
                if (roles.Contains("Staff") || (await _tradeService.IsTradeDetailsOwner(userId, dto.TradeDetailsId)))
                {
                    return BadRequest("You don't have permission to do this!");
                }
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        _unitOfWork.BookCheckListService.UpdateCheckList(updatedData);
                        int changes = await _unitOfWork.Save();
                        await transaction.CommitAsync();
                        return Ok(changes);
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("An error occurred while updating check list! Exception: " + e.Message);
                    }
                }
            }
            if (dto.BookOwnerUploadDir != null)
            {
                if (roles.Contains("Staff") || !(await _tradeService.IsTradeDetailsOwner(userId, dto.TradeDetailsId)))
                {
                    return BadRequest("You don't have permission to do this!");
                }

                string? imgUrl = null;
                CloudinaryResponseDTO cloudRsp
                        = _cloudinaryService.UploadImage(dto.BookOwnerUploadDir, "Checklist/Owner/" + updatedData.Id);
                if (cloudRsp.StatusCode == 200 && cloudRsp.Data != null)
                {
                    imgUrl = cloudRsp.Data;
                }

                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        if (imgUrl != null) { updatedData.BookOwnerUploadDir = imgUrl; }
                        _unitOfWork.BookCheckListService.UpdateCheckList(updatedData);
                        int changes = await _unitOfWork.Save();
                        await transaction.CommitAsync();
                        return Ok(changes);
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("An error occurred while updating check list! Exception: " + e.Message);
                    }
                }
            }
            if (dto.MiddleUploadDir != null)
            {
                if (!roles.Contains("Staff"))
                {
                    return BadRequest("You have to be a staff to do this!");
                }
                string? imgUrl = null;
                CloudinaryResponseDTO cloudRsp
                        = _cloudinaryService.UploadImage(dto.MiddleUploadDir, "Checklist/Middle/" + updatedData.Id);
                if (cloudRsp.StatusCode == 200 && cloudRsp.Data != null)
                {
                    imgUrl = cloudRsp.Data;
                }

                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        if (imgUrl != null) { updatedData.BookOwnerUploadDir = imgUrl; }
                        _unitOfWork.BookCheckListService.UpdateCheckList(updatedData);
                        int changes = await _unitOfWork.Save();
                        await transaction.CommitAsync();
                        return Ok(changes);
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("An error occurred while updating check list! Exception: " + e.Message);
                    }
                }
            }
            return Ok();
        }
    }
}

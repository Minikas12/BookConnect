using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;
using Microsoft.AspNetCore.Mvc;
using BusinessObjects.Enums;
using Newtonsoft.Json;
using BusinessObjects.Models.Ecom.Rating;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IAccountService _accountService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IAddressService _addressService;
        private readonly ITradeService _tradeService;
        private readonly IRatingService _ratingService;
        private readonly IMapper _mapper;

        public PostController(IMapper mapper, IPostService postService, ICloudinaryService cloudinaryService, IAccountService accountService,
            IAddressService addressService, ITradeService tradeService, IRatingService ratingService)
        {
            _mapper = mapper;
            _ratingService = ratingService;
            _cloudinaryService = cloudinaryService;
            _addressService = addressService;
            _postService = postService;
            _accountService = accountService;
            _tradeService = tradeService;
        }
        //---------------------------------------------POST-------------------------------------------------------//

        [HttpGet("get-all-post")]
        public async Task<IActionResult> GetAllPostAsync([FromQuery] PagingParams @params)
        {
            var posts = await _postService.GetAllPostAsync(@params);

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

        [HttpGet("get-post-by-user")]
        public async Task<IActionResult> GetPostByUserIdAsync(Guid userId, [FromQuery] PagingParams @params)
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId, @params);

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

        // get post by id
        [HttpGet("get-post-by-id")]
        public async Task<IActionResult> GetPostByIdAsynsc(Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            PostDetailsDTO result = new PostDetailsDTO();
            if (post != null) {
                var user = await _accountService.FindUserByIdAsync(post.UserId);
                if (user != null)
                {
                    if(post.Content == null)
                    {
                        post.Content = "";
                    }
                    result.PostData = post;
                    result.Username = user.Username;
                    result.AvatarDir = user.AvatarDir;
                    result.ReadingTime = _postService.CalculateReadingTime(post.Content);
                    result.Tags = _mapper.Map<List<CateNameAndIdDTO>>(await _postService.GetAllCategoryOfPostAsync(post.PostId));
                }
             }
            return Ok(result);
        }

        [HttpPut("update-post"), Authorize]
        public async Task<IActionResult> UpdatePostAsync([FromForm] UpdatePostDTOs dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model state invalid");
            }
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            if (!await _postService.IsPostOwnerAsync(dto.PostId, userId))
            {
                return BadRequest("Not post owner!");
            }

            string? user = await _accountService.GetUsernameById(userId);

            var updateData = _mapper.Map<Post>(dto);

            updateData.UserId = userId;
            if (dto.Image != null)
            {
                string? oldImgPath = await _postService.GetOldImgPathAsync(dto.PostId);

                if (oldImgPath != null && oldImgPath != "")
                {
                    _cloudinaryService.DeleteImage(oldImgPath, "Post");
                }
                var cloudResponse = _cloudinaryService.UploadImage(dto.Image, "Posts/" + user + "/" + dto.PostId + "/Images");
                if (cloudResponse.StatusCode != 200 || cloudResponse.Data == null)
                {
                    return BadRequest(cloudResponse.Message);
                }
                updateData.ImageDir = cloudResponse.Data;
            }
            //if (dto.ProductVideos != null)
            //{
            //    string? oldVidPath = await _postService.GetOldVideoPathAsync(dto.PostId);

            //    if (oldVidPath != null && oldVidPath != "")
            //    {
            //        _cloudinaryService.DeleteVideo(oldVidPath, "Post");
            //    }
            //    var cloudResponse = _cloudinaryService.UploadVideo(dto.ProductVideos, "Posts/" + userPost + "/" + dto.PostId + "/Videos");
            //    if (cloudResponse.StatusCode != 200 || cloudResponse.Data == null)
            //    {
            //        return BadRequest(cloudResponse.Message);
            //    }
            //    updateData.VideoDir = cloudResponse.Data;
            //}

            if ((await _postService.UpdatePostAsync(updateData)) > 0)
            {
                return Ok("Successful");
            }
            return BadRequest("Update fail");


        }


        [HttpPost("add-new-post"), Authorize]
        public async Task<IActionResult> AddNewPostAsync([FromForm] AddPostDTOs dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model is invalid!");
            }
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            Guid postId = Guid.NewGuid();
            string? userPost = await _accountService.GetUsernameById(userId);
            string imageDir = "";
            string videoDir = "";

            if (dto.ProductImages != null)
            {
                var saveProductResult = _cloudinaryService.UploadImage(dto.ProductImages, $"Posts/{userPost}/{postId}/Images");
                if (saveProductResult.StatusCode != 200 || saveProductResult.Data == null)
                {
                    return BadRequest(saveProductResult.Message);
                }
                imageDir = saveProductResult.Data;
            }

            if (dto.ProductVideos != null)
            {
                var saveProductResult = _cloudinaryService.UploadVideo(dto.ProductVideos, $"Posts/{userPost}/{postId}/Videos");
                if (saveProductResult.StatusCode != 200 || saveProductResult.Data == null)
                {
                    return BadRequest(saveProductResult.Message);
                }
                videoDir = saveProductResult.Data;
            }

            var newPost = new Post()
            {
                UserId = userId,
                PostId = postId,
                Title = dto.Title,
                Content = dto.Content,
                IsTradePost = dto.IsTradePost,
                ImageDir = imageDir,
                VideoDir = videoDir,
                CreatedAt = DateTime.Now
            };

            int result = await _postService.AddNewPostAsync(newPost);

            if (result > 0)
            {
                int changes = 0;
                if (dto.CateIds != null)
                {
                    changes += await _postService.AddPostToCategoriesAsync(postId, dto.CateIds);
                }
                return (changes > 0) ? Ok(newPost.PostId) : Ok("Post create successful, but no category were added! " + newPost.PostId);
            }

            return BadRequest("Failed to add the post");
        }
   
        [HttpDelete("delete-post")]
        public async Task<IActionResult> DeletePostByIdAsync(Guid postId)
        {
            try
            {
                string oldVid = await _postService.GetOldVideoPathAsync(postId);
                string oldImg = await _postService.GetOldImgPathAsync(postId);
                CloudinaryResponseDTO res = new CloudinaryResponseDTO();

                if (oldVid != "" && oldVid != null)
                {
                   res = _cloudinaryService.DeleteVideo(oldVid, "Post");
                }

                if (oldImg != "" && oldImg != null)
                {
                    _cloudinaryService.DeleteImage(oldImg, "Post");
                }

                int changes = await _postService.DeletePostByIdAsync(postId);
                IActionResult result = (changes > 0) ? Ok("Successful! " + res.StatusCode  + res.Message + res.Data) : BadRequest("Delete fail!");
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        //---------------------------------------------COMMENT-------------------------------------------------------//

        [HttpGet("get-comment-by-post-id")]
        public async Task<IActionResult> GetCommentByPostIdAsync(Guid postId, [FromQuery] PagingParams @params)
        {
            try
            {
                var comment = await _postService.GetCommentByPostIdAsync(postId, @params);

                if (comment != null)
                {
                    var metadata = new
                    {
                        comment.TotalCount,
                        comment.PageSize,
                        comment.CurrentPage,
                        comment.TotalPages,
                        comment.HasNext,
                        comment.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(comment);
                }
                else return BadRequest("No commen's found!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPost("add-comment")]
        public async Task<IActionResult> AddCommenAsync([FromBody] AddCommentDTO comment)
        {
                if (ModelState.IsValid)
                {
                    CommentDetailsDTO apiRespone = new CommentDetailsDTO();
                    Guid cmtId = Guid.NewGuid();
                    DateTime createDate = DateTime.Now;
                    var commenter = await _accountService.FindUserByIdAsync(comment.CommenterId);
                    int cmtChanges = await _postService.AddCommentAsync(new Comment()
                    {
                        CommentId = cmtId,
                        CommenterId = comment.CommenterId,
                        Content = comment.Content,
                        CreateDate = createDate,
                    });
                    if (cmtChanges > 0)
                    {
                    int recordChanges = await _postService.AddNewCommentRecord(cmtId, comment.PostId);
                    if (recordChanges > 0)
                    {
                        apiRespone.CommentId = cmtId;
                        apiRespone.CommenterId = comment.CommenterId;
                        apiRespone.Content = comment.Content;
                        apiRespone.CreateDate = createDate;
                        apiRespone.PostId = comment.PostId;

                        if(commenter != null)
                        {
                            apiRespone.Username = commenter.Username;
                            apiRespone.AvatarDir = commenter.AvatarDir;
                        }
                        return Ok(apiRespone);
                    }
                    else return Ok("Fail to add comment record, Deleted comments: " + await _postService.DeleteCommentByIdAsync(cmtId));
                    }
                    return BadRequest("Add fail!");
                }
                return BadRequest("Model invalid!");
        }

        //    [HttpPut("Update-Comment")]
        //    public IActionResult UpdateComment([FromForm] UpdateCommentDTO comment)
        //    {
        //        try
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                Comment updateData = new Comment
        //                {
        //                    CommentId = comment.CommentId,
        //                    PostId = comment.PostId,
        //                    CommenterId = comment.CommenterId,
        //                    Description = comment.Description,
        //                    Created = DateTime.Now
        //                };
        //                if (_postService.UpdateComment(updateData) > 0)
        //                {
        //                    return Ok("Successful");
        //                }
        //                return BadRequest("Update fail");
        //            }
        //            return BadRequest("Model state invalid");
        //        }
        //        catch (Exception e)
        //        {
        //            throw new Exception(e.Message);
        //        }
        //    }

        [HttpDelete("delete-comment")]
        public async Task<IActionResult> DeleteCommentById(Guid commentId)
        {
            try
            {
                int cmtChanges = await _postService.DeleteCommentByIdAsync(commentId);
                var response = new
                    {
                        StatusCode = 204,
                        Message = "Delete comment query was successful",
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

        ///*------------------------------------User Saved Post------------------------------------*/

        [HttpPost("add-new-user-saved-post"), Authorize]
        public async Task<IActionResult> AddNewUserSavedPostAsync(Guid postId)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);
            if (await _postService.IsAlreadySavedAsync(userId, postId))
            {
                return BadRequest("Post's already saved!");
            }
            int changes = await _postService.AddNewUserSavedPostAsync(userId, postId);

            return (changes > 0) ? Ok("Successful!") : Ok("No changes!");
        }

        [HttpGet("get-user-saved-posts")]
        public async Task<IActionResult> GetUserSavedPostAsync([FromQuery] PagingParams @params)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var posts = await _postService.GetUserSavedPostAsync(userId, @params);

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

        [HttpDelete("remove-user-saved-post"), Authorize]
        public async Task<IActionResult> RemoveUserSavedPost(Guid postId)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            int changes = await _postService.RemoveUserSavedPostAsync(userId, postId);
            return (changes > 0) ? Ok("Successful!") : Ok("No changes!");
        }
        //[HttpPut("ban-post")]
        //public async Task<ActionResult<int>> BanPostAsync(Guid postId)
        //{
        //    var result = await _postService.BanPostAsync(postId);
        //    return Ok(result);
        //}
    }
}
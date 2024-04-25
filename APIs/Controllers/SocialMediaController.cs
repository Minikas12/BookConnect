using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using AutoMapper;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialMediaController : ControllerBase
    {

        private readonly ISocialMediaService _socialMediaService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public SocialMediaController(IMapper mapper, ISocialMediaService socialMediaService, IPostService postService, ICategoryService categoryService)
        {
            _mapper = mapper;
            _socialMediaService = socialMediaService;
            _categoryService = categoryService;
            _postService = postService;
        }

        [HttpPost("add-user-targeted-category"), Authorize]
        public async Task<IActionResult> AddNewUserTargetedCategoryByList([FromBody] List<Guid> cateIdList)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);
            List<Guid> afterFilterIds = new List<Guid>();

            foreach(var c in cateIdList)
            {
              if(!await _socialMediaService.IsAlreadyTargetedAsync(userId, c))
                {
                    afterFilterIds.Add(c);
                }
            }

            int changes = await _socialMediaService.AddUserTargetCategoryByListAsync(afterFilterIds, userId);
            return (changes > 0) ? Ok("Successful! Number of cate added: " + changes) : Ok("No changes!");
        }

        [HttpGet("get-user-targeted-categories"), Authorize]
        public async Task<IActionResult> GetUTCByUserIdAsync()
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var targeted = await _socialMediaService.GetUTCByUserIdAsync(userId);

            var result = new List<GetUTCByUserIdResponseDTO>();

            foreach (var t in targeted)
            {
                result.Add(new GetUTCByUserIdResponseDTO
                {
                    CateId = t.CategoryId,
                    CateName = (await _categoryService.GetCategoryByIdAsync(t.CategoryId))?.CateName,
                });
            }
            return Ok(result);
        }

        [HttpDelete("delete-user-targeted-categories"), Authorize]
        public async Task<IActionResult> RemoveUserTargetedCategoryAsync(Guid cateId)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);
            int changes = await _socialMediaService.RemoveUserTargetedCategoryAsync(userId, cateId);
            return (changes > 0) ? Ok("Successful!") : Ok("No changes!");
        }

        [HttpPost("add-post-to-category")]
        public async Task<IActionResult> AddPostToCategoryAsync([FromBody] AddPostToCategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model invalid!");
            }

            int changes = await _postService.AddPostToCategoriesAsync(dto.PostId, dto.CategoryIds);
            return (changes > 0) ? Ok("Successful! Number of changes: " + changes) : Ok("No changes!");
        }

        [HttpGet("get-category-of-a-post")]
        public async Task<IActionResult> GetAllCategoryOfPostAsync(Guid postId)
        {
            var categories = await _postService.GetAllCategoryOfPostAsync(postId);

            return Ok(categories);
        }
    
        [HttpDelete("remove-post-from-category")]
        public async Task<IActionResult> RemovePostfromCategoryAsync([FromBody] RemovePostfromCategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model invalid!");
            }
            int changes = await _postService.RemovePostfromCategoryAsync(dto.PostId, dto.CategoryId);
            return (changes > 0) ? Ok("Successful!") : Ok("No changes!");
        }

        [HttpGet("get-post-by-user-targeted-categories"), Authorize]
        public async Task<IActionResult> GetPostByUserTargetedCategoriesAsync([FromQuery] PagingParams @params)
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var posts = await _postService.GetPostByUserTargetedCategoriesAsync(userId, @params);

            var result = await _postService.ConvertToPostDetailsListAsync(posts);

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

        [HttpPost("add-social-tag")]
        public async Task<IActionResult> AddPostSocialTag([FromBody] AddPostSocialTagRequestDTO dto)
        {
            int changes = 0;
            if (!ModelState.IsValid)
            {
                return BadRequest("Model invalid!");
            }
            if (!await _postService.IsPostExisted(dto.PostId)){
                return BadRequest("Post doesn't existed!");
            }

            List<Guid> addedTags = new List<Guid>();

            foreach(var t in dto.TagNames)
            {
                Guid? tagId = await _categoryService.GetSocialTagIdByName(t);
                if(tagId == null)
                {
                    tagId = Guid.NewGuid();
                    await _categoryService.AddCategoryAsync(new Category
                    {
                        CateId = (Guid)tagId,
                        CateName = t,
                        CreateDate = DateTime.Now,
                        ImageDir = "",
                        IsSocialTag = true
                    });
                }
                addedTags.Add((Guid)tagId);
            }
            changes += await _postService.AddPostToCategoriesAsync(dto.PostId, addedTags);

            return (changes == dto.TagNames.Count) ? Ok("Successful!") : Ok("Fail to add " + (dto.TagNames.Count - changes) + " tags");
        }

        [HttpGet("get-all-saved-post-tags-by-userId"), Authorize]
        public async Task<IActionResult> GetAllSavedPostTags()
        {
            var userIdClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return BadRequest("User is not authenticated!");
            }
            var userId = Guid.Parse(userIdClaim.Value);
            var result = _mapper.Map<List<CateNameAndIdDTO>>(await _socialMediaService.GetAllSavedPostTagsAsync(userId));
            return Ok(result);
        }
    }
}


using System;
using APIs.Services;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;
using DataAccess.DAO;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookGroupController : ControllerBase
    {
        private readonly IBookGroupService _bookGroupServices;
        private readonly ICloudinaryService _clouinaryService;
        private readonly IAccountService _accService;

        public BookGroupController(IBookGroupService bookGroupServices, ICloudinaryService cloudinaryService, IAccountService accService)
        {
            _bookGroupServices = bookGroupServices;
            _clouinaryService = cloudinaryService;
            _accService = accService;
        }

        [HttpPost("AddBookGroup")]
        public IActionResult AddNewBookGroup([FromForm] NewBookGroupDTO dto)
        {
            try
            {
                // Retrieve ownerId from session

                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id not found for the given owner");
                }
                if (ModelState.IsValid)
                {
                    string imgUrl = string.Empty;
                    if (dto.bookGroupImg != null)
                    {
                        CloudinaryResponseDTO cloudRsp
                        = _clouinaryService.UploadImage(dto.bookGroupImg, "BookGroups/Book");
                        if (cloudRsp.StatusCode == 200 && cloudRsp.Data != null)
                        {
                            imgUrl = cloudRsp.Data;
                        }
                    }
                    BookGroup bookGroup = new BookGroup
                    {
                        BookGroupId = Guid.NewGuid(),
                        BookGroupName = dto.BookGroupName,
                        ImageDir = imgUrl,
                        Description = dto.Description,
                        AgencyId = agencyId
                    };
                    int changes = _bookGroupServices.AddBookGroup(bookGroup);
                    IActionResult result = (changes > 0) ? Ok(bookGroup.BookGroupId) : BadRequest("Add fail!");
                    return result;
                }
                return BadRequest("Model invalid!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [EnableQuery()]
        [HttpGet("GetAllBookGroup")]
        public IActionResult GetAllBookGroup([FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookGroupServices.GetAllBookGroup(@params);


                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        [HttpPut("UpdateBookGroup")]
        public IActionResult UpdateBookGroup([FromForm] NewBookGroupDTO dto)
        {
            try
            {

                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id not found for the given owner");
                }
                if (ModelState.IsValid)
                {
                    if (dto.BookGroupId == null)
                    {
                        return BadRequest("Book Group id is cannot be null!");
                    }
                    string imgUrl = _bookGroupServices.GetOldImgPath((Guid)dto.BookGroupId);
                    if (dto.bookGroupImg != null)
                    {
                        string oldImg = imgUrl;

                        if (oldImg != "")
                        {
                            _clouinaryService.DeleteImage(oldImg, "BookGroups/Book");
                        }
                        CloudinaryResponseDTO cloudRsp
                        = _clouinaryService.UploadImage(dto.bookGroupImg, "BookGroups/Book");
                        if (cloudRsp.StatusCode == 200 && cloudRsp.Data != null)
                        {
                            imgUrl = cloudRsp.Data;
                        }
                    }
                    BookGroup existingBG = _bookGroupServices.GetBookGroupById((Guid)dto.BookGroupId);
                    BookGroup bookGroup = new BookGroup
                    {
                        BookGroupId = (Guid)dto.BookGroupId,
                        BookGroupName = dto?.BookGroupName ?? existingBG.BookGroupName,
                        ImageDir = imgUrl,
                        Description = dto?.Description ?? existingBG.Description,
                        AgencyId = agencyId
                    };

                    int changes = _bookGroupServices.UpdateBookGroup(bookGroup);
                    IActionResult result = (changes > 0) ? Ok("Successful!") : BadRequest("Update fail!");
                    return result;

                }
                return BadRequest("Model invalid!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("GetBookGroupById")]
        public IActionResult GetBookGroupById(Guid bookGroupId)
        {
            try
            {
                return Ok(_bookGroupServices.GetBookGroupById(bookGroupId));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpDelete("DeleteBookGroup")]
        public IActionResult DeleteBookGroup(Guid bookGroupId)
        {
            try
            {

                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id not found for the given owner");
                }
                string imgUrl = string.Empty;
                string oldImg = _bookGroupServices.GetOldImgPath(bookGroupId);

                if (oldImg != "")
                {
                    _clouinaryService.DeleteImage(oldImg, "Categories/Book");
                }
                int changes = _bookGroupServices.DeleteBookGroup(bookGroupId);
                IActionResult result = (changes > 0) ? Ok("Successful!") : BadRequest("Delete fail!");
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("SearchBookGroup")]
        public IActionResult SearchBookGroup(string? inputString, [FromQuery] PagingParams param)
        {
            try
            {
                var cates = _bookGroupServices.GetAllBookGroup(param);
                if (inputString != null && inputString != "")
                {
                    cates = _bookGroupServices.GetBookGroupByName(inputString, param);
                }
                if (cates != null)
                {
                    var metadata = new
                    {
                        cates.TotalCount,
                        cates.PageSize,
                        cates.CurrentPage,
                        cates.TotalPages,
                        cates.HasNext,
                        cates.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(cates);
                }
                return Ok(cates);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
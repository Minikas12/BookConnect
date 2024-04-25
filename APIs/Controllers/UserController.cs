using APIs.Services.Interfaces;
using AutoMapper;
using BusinessObjects.DTO;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
	[ApiController, Route("api/user")]
	public class UserController: ControllerBase
	{
		private readonly IAccountService _accountService;
		private readonly IAdminService _adminService;
		private readonly IMapper _mapper;
		public UserController(IAccountService accountService,IAdminService adminService, IMapper mapper)
		{
			_accountService = accountService;
			_adminService = adminService;
			_mapper = mapper;
		}

		[HttpGet("get-user-by-userId")]
		public async Task<IActionResult> GetUserById(Guid userId)
		{
			var user = await _accountService.FindUserByIdAsync(userId);
			UserProfileDTO? result = null;
			if (user != null)
			{
				result = _mapper.Map<UserProfileDTO>(user);
                result.Roles = new List<string>();
                var roles = await _adminService.GetAllUserRolesAsync(userId);
				foreach (var r in roles)
				{
					result.Roles.Add(r.Value);
				}
				result.Address = _accountService.GetDefaultAddress(userId)?.Rendezvous;
				result.Agencies = _accountService.GetOwnerAgencies(userId);
			}

			return Ok(result);
		}

        [HttpGet("get-user-by-username")]
        public async Task<IActionResult> GetUserByName(string username)
        {
            var user = await _accountService.FindUserByUsernameAsync(username);
            UserProfileDTO? result = null;
            if (user != null)
            {
                result = _mapper.Map<UserProfileDTO>(user);
                result.Roles = new List<string>();
                var roles = await _adminService.GetAllUserRolesAsync(user.UserId);
                foreach (var r in roles)
                {
                    result.Roles.Add(r.Value);
                }
                result.Address = _accountService.GetDefaultAddress(user.UserId)?.Rendezvous;
                result.Agencies = _accountService.GetOwnerAgencies(user.UserId);
            }

            return Ok(result);
        }

	}
}


using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;
using DataAccess.DTO;
using Microsoft.AspNetCore.Identity;

namespace APIs.Services.Interfaces
{
    public interface IAccountService
    {
        //User services
        Task<IdentityResult> ChangePassword(PasswordChangeDTO model);
        Task<AppUser?> FindUserByEmailAsync(string email);
        Task<AppUser> Register(RegisterDTO model);
        bool VerifyPassword(string pwd, string hash, byte[] salt);
        Task<string> CreateTokenAsync(AppUser user);
        Task<RefreshToken?> GenerateRefreshTokenAsync(Guid userId);
        Task<Role?> GetRoleDetails(string roleName);
        Task<string?> GetUsernameById(Guid userId);
        Task<bool> IsBanned(Guid userId);
        Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
        Task<AppUser?> FindUserByIdAsync(Guid userId);
        Task<AppUser?> FindUserByUsernameAsync(string username);
        Task<int> AddNicData(NIC_Data data);

        //Address services
        List<Address> GetAllUserAdderess(Guid userId);
        Address GetDefaultAddress(Guid userId);

        //Validate services
        Task<int> SetUserIsValidated(bool choice, Guid userId);
        Task<bool> IsUserValidated(Guid userId);

        //Agency registration
        List<Agency> GetOwnerAgencies(Guid ownerId);
        Task<string> RegisterAgency(AgencyRegistrationDTO dto, string logoUrl, Guid ownerId);
        //bool IsSeller(Guid userId);
        Agency GetAgencyById(Guid agencyId);
        int UpdateAgency(AgencyUpdateDTO updatedData, string? logoUrl);

        Task<int> UpdateProfile(AppUser appUser); //SONDB 
        Task<UserPublicProfileDTO> GetUserById(Guid userId); //SONDB 
        Task<PagedList<AppUser>> GetAllUsers(PagingParams param);
        Task<string?> GetUserAvatarDirById(Guid userId); //SONDB 
        Task<string?> GetUserPhoneById(Guid userId); //SONDB 
        Task<bool> HasBookInInventory(Guid agencyId, Guid bookId);
        Task<double> CalculateOverallReplyPercentageForAgencyAsync(Guid agencyId);
        Task<Agency> GetAgencyByUserIdAsync(Guid userId);
        Task<Agency> GetAgencyByIdAsync(Guid agencyId);
        Task<Guid> GetAgencyIdAsync(Guid ownerId);
        Task SendVerificationEmail(string email, Guid userId);
        //Task<bool> VerifyEmail(Guid userId);
        //Task<bool> IsEmailVerified(string email);

        // ---------------------------DATDQ-----------------------------------//
        decimal GetTotalRevenueByTime(Guid agencyId, DateTime startDate, DateTime endDate);
        AgencyAnalystDTO GetAgencyAnalyst(Guid agencyId);
        Guid GetAgencyId(Guid ownerId);
        Guid GetInventoryId(Guid agencyId);
        AgencyAnalystTimeInputDTO GetAgencyAnalystByTime(Guid agencyId, DateTime startDate, DateTime endDate);

        // ---------------------END DATDQ------------------------------------//

    }

}


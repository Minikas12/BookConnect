using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;
using DataAccess.DAO;
using DataAccess.DAO.Ecom;
using DataAccess.DAO.Utils;
using DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MailKit.Net.Smtp;


namespace APIs.Repositories.Interfaces
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _config;

        private readonly AccountDAO _accountDAO;
        private readonly AddressDAO _addressDAO;
        private readonly RoleRecordDAO _roleRecordDAO;
        private readonly RoleDAO _roleDAO;
        private readonly RatingDAO _ratingDAO;
        private readonly AgencyDAO _agencyDAO;
        private readonly RefreshTokenDAO _refreshTokenDAO;
        private readonly NICDAO _NicDAO;

        public AccountService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _NicDAO = new NICDAO(context);
            _addressDAO = new AddressDAO();
            _roleDAO = new RoleDAO();
            _roleRecordDAO = new RoleRecordDAO();
            _ratingDAO = new RatingDAO();
            _accountDAO = new AccountDAO();
            _agencyDAO = new AgencyDAO();
            _refreshTokenDAO = new RefreshTokenDAO();
        }

        //User services
        public Task<IdentityResult> ChangePassword(PasswordChangeDTO model)
        {
            throw new NotImplementedException();
        }

        public async Task<AppUser> Register(RegisterDTO model)
        {
            HashPassword(model.Password, out byte[] salt, out byte[] pwdHash);
            AppUser user = new AppUser()
            {
                UserId = Guid.NewGuid(),
                Username = model.Username,
                Email = model.Email,
                Password = Convert.ToHexString(pwdHash),
                Salt = Convert.ToHexString(salt),
                IsValidated = false,
            };
            await _accountDAO.CreateAccount(user);
            await _roleRecordDAO.AddNewRoleRecordAsync(user.UserId,Guid.Parse("2da9143d-559c-40b5-907d-0d9c8d714c6c"));
            await SendVerificationEmail(model.Email, user.UserId);
            return user;
        }

       


        public async Task<AppUser?> FindUserByEmailAsync(string email) => await _accountDAO.FindUserByEmailAsync(email);

        public async Task<AppUser?> FindUserByIdAsync(Guid userId) => await _accountDAO.FindUserByIdAsync(userId);

        public async Task<AppUser?> FindUserByUsernameAsync(string username) => await _accountDAO.FindUserByUsernameAsync(username);

        public async Task<string> CreateTokenAsync(AppUser user)
        {
            Dictionary<Guid, string> roles = await _roleDAO.GetAllUserRolesAsync(user.UserId);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username),
                //new Claim(ClaimTypes.Role, new RoleDAO().GetRoleById(user.RoleId).RoleName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("userId", user.UserId.ToString()),
            };

            foreach(var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r.Value));
            }

            string? pepper = _config.GetSection("JWT:Pepper").Value;
            if (pepper == null)
            {
                return "Pepper not found!";
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(pepper));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: "Book connect",
                audience: "Pikachu",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: cred
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }    
        public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token) => await _refreshTokenDAO.ValidateRefreshTokenAsync(token);

        public async Task<RefreshToken?> GenerateRefreshTokenAsync(Guid userId)
        {
            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreatedDate = DateTime.Now,
                ExpiredDate = DateTime.Now.AddMinutes(30)
            };
            int changes = await _refreshTokenDAO.AddRefreshTokenAsync(token);
            RefreshToken? result = (changes > 0) ? token : null;
            return result;
        }

        public bool VerifyPassword(string pwd, string hash, byte[] salt)
        {
            const int keySize = 64;
            const int iterations = 360000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(pwd),
                salt,
                iterations,
                hashAlgorithm,
                keySize);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
        }

        private void HashPassword(string pwd, out byte[] salt, out byte[] pwdHash)
        {
            const int keySize = 64;
            const int iterations = 360000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(pwd),
                salt,
                iterations,
                hashAlgorithm,
                keySize);
            pwdHash = hash;
        }

        public async Task<Role?> GetRoleDetails(string roleName) => await _roleDAO.GetRolesDetails(roleName);

        public async Task<string?> GetUsernameById(Guid userId) => await _accountDAO.GetNameById(userId);

        //Address services
        public List<Address> GetAllUserAdderess(Guid userId) => new AddressDAO().GetAllUserAddress(userId);

        public Address? GetDefaultAddress(Guid userId) => new AddressDAO().GetUserDefaultAddress(userId);

        //Account validation
        public async Task<int> SetUserIsValidated(bool choice, Guid userId) => await _accountDAO.SetIsAccountValid(choice, userId);

        public async Task<bool> IsUserValidated(Guid userId) => await _accountDAO.IsUserValidated(userId);

        //Agency Registration
        public List<Agency> GetOwnerAgencies(Guid ownerId) => new AgencyDAO().GetAgencyByOwnerId(ownerId);


        public async Task<string> RegisterAgency(AgencyRegistrationDTO dto, string logoUrl, Guid ownerId)
        {
                int newAgency = _agencyDAO.AddNewAgency(new Agency
                {
                    AgencyId = Guid.NewGuid(),
                    AgencyName = dto.AgencyName,
                    PostAddressId = dto.AddressId,
                    LogoUrl = logoUrl,
                    BusinessType = dto.BusinessType,
                    OwnerId = ownerId
                });
                if (newAgency > 0)
                {
                    await _roleRecordDAO.AddNewRoleRecordAsync(ownerId, Guid.Parse("439e2d3c-6050-4480-a7d5-ab4b23425992"));
                    return "Successful!";
                }
                else return "Fail to add new agency!";
        }


        //public async Task<bool> IsSeller(Guid userId) => await _accountDAO.IsSeller(userId);

        public Task<bool> IsBanned(Guid userId) => new AccountDAO().IsBanned(userId);

        public Agency GetAgencyById(Guid agencyId) => new AgencyDAO().GetAgencyById(agencyId);

        public int UpdateAgency(AgencyUpdateDTO updatedData, string? updatedLogoUrl)
        {
            AddressDAO addressDAO = new AddressDAO();
            AgencyDAO agencyDAO = new AgencyDAO();
            Guid addressId = Guid.Empty;

            Address? oldAddress = agencyDAO.GetCurrentAddress(updatedData.AgencyId);
            if(oldAddress != null) addressId = oldAddress.AddressId;

            if (oldAddress != null && oldAddress.Rendezvous != null && updatedData.PostAddress != null)
            {
                if(!oldAddress.Rendezvous.Equals(updatedData.PostAddress))
                {
                    Address newAddress = new Address
                    {
                        AddressId = Guid.NewGuid(),
                        City_Province = null,
                        District = null,
                        SubDistrict = null,
                        Rendezvous = updatedData.PostAddress
                    };
                    if (addressDAO.AddNewAddress(newAddress) > 0) addressId = newAddress.AddressId;
                }
            }

            string? logoUrl = (updatedLogoUrl != null) ? updatedLogoUrl : agencyDAO.GetCurrentLogoUrl(updatedData.AgencyId);

            return agencyDAO.UpadateAgency(new Agency
            {
                AgencyId = updatedData.AgencyId,
                AgencyName = updatedData.AgencyName,
                PostAddressId = addressId,
                BusinessType = updatedData.BusinessType,
                LogoUrl = logoUrl,
                OwnerId = updatedData.OwnerId,
            });
        }
        public async Task<int> UpdateProfile(AppUser user) => (await new AccountDAO().UpdateProfileAsync(user)); //SONDB 
        public async Task <UserPublicProfileDTO> GetUserById(Guid userId) => (await new AccountDAO().GetUserById(userId)); //SONDB 
        public async Task<PagedList<AppUser>> GetAllUsers(PagingParams param) 
        {
            return PagedList<AppUser>.ToPagedList((await _accountDAO.GetAllUsers()).OrderBy(c => c.UserId).AsQueryable(), param.PageNumber, param.PageSize);
        }



        public async Task<string?> GetUserAvatarDirById(Guid userId)//SONDB 
        {
            return await _accountDAO.GetUserAvatarDirById(userId);
        }


        public async Task<string?> GetUserPhoneById(Guid userId) //SONDB 
        {
            return await _accountDAO.GetUserPhoneById(userId);

        }
        public async Task<bool> HasBookInInventory(Guid agencyId, Guid bookId) => await _agencyDAO.HasBookInInventory(agencyId, bookId); //SONDB 
        public async Task<double> CalculateOverallReplyPercentageForAgencyAsync(Guid agencyId) => await _ratingDAO.CalculateOverallReplyPercentageForAgencyAsync(agencyId); //SONDB 

        public Task<Agency> GetAgencyByUserIdAsync(Guid userId) 
        {
            throw new NotImplementedException();
        }

        public async Task<Agency> GetAgencyByIdAsync(Guid agencyId) => await _agencyDAO.GetAgencyByIdAsync(agencyId);

        public async Task<int> AddNicData(NIC_Data data)
        => await _NicDAO.AddNicData(data);
        public async Task<Guid> GetAgencyIdAsync(Guid ownerId) => await _agencyDAO.GetAgencyIdAsync((Guid)ownerId);
        //public async Task<bool> VerifyEmail(Guid userId) => await _accountDAO.VerifyEmail(userId);

        //public async Task<bool> IsEmailVerified(string email) => await _accountDAO.IsEmailVerified(email);
        public async Task SendVerificationEmail(string email, Guid userId)
        {
            try
            {
                // Create an instance of MimeMessage
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Book Connect", "destroykurusaki@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Verify Your Email Address";

                // Generate confirmation link
                string confirmationLink = $"https://localhost:7138/api/Account/verify-email/{userId}";

                // Build email body with verification link
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $"<p>Please click <a href=\"{confirmationLink}\">here</a> to verify your email address.</p>";
                message.Body = bodyBuilder.ToMessageBody();

                // Create an instance of SmtpClient with your SMTP settings
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 465, true);

                // Authenticate with the SMTP server
                await client.AuthenticateAsync("destroykurusaki@gmail.com", "kixm azxh vtix duoy");

                // Send the email
                await client.SendAsync(message);

                // Disconnect from the SMTP server
                await client.DisconnectAsync(true);

                Console.WriteLine("Verification email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send verification email: {ex.Message}");
            }
        }

        // --------------------------DATDQ--------------------------------//
        public Guid GetAgencyId(Guid ownerId) => new AgencyDAO().GetAgencyId(ownerId);
        public Guid GetInventoryId(Guid agencyId) => new AgencyDAO().GetInventoryId(agencyId);
        OrderDAO o = new OrderDAO();
        AgencyDAO a = new AgencyDAO();
        public AgencyAnalystDTO GetAgencyAnalyst(Guid agencyId) => a.GetAgencyAnalyst(agencyId);
        public AgencyAnalystTimeInputDTO GetAgencyAnalystByTime(Guid agencyId, DateTime startDate, DateTime endDate) => a.GetAgencyAnalystByTime(agencyId, startDate, endDate);
        public decimal GetTotalRevenueByTime(Guid agencyId, DateTime startDate, DateTime endDate) => o.GetTotalRevenueByTime(agencyId, startDate, endDate);

        // --------------------------END DATDQ----------------------------//

    }

}

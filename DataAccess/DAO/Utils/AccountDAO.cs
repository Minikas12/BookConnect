using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
	public class AccountDAO
	{
        private readonly AppDbContext _context;
        public AccountDAO()
        {
            _context = new AppDbContext();
        }

        /*---------------------------------------APPUSER-------------------------------------------*/
        /*------------------BEGIN GET-------------------*/

        public async Task<List<AppUser>> GetAllUsers() => await _context.AppUsers.ToListAsync();
       
        public async Task<string?> GetAccountSalt(Guid userId)
        {
            AppUser? user = await _context.AppUsers.SingleOrDefaultAsync(u => u.UserId == userId);
            string? salt = (user != null) ? user.Salt : null;
            return salt;
        }
        public async Task<AppUser?> FindUserByEmailAsync(string email)
        {
            AppUser? user =  (await CheckEmail(email)) ? await _context.AppUsers.SingleOrDefaultAsync(u => u.Email == email) : null;
            return user;
        }

        public async Task<string?> GetNameById(Guid userId)
        {
            AppUser? user = await _context.AppUsers.SingleOrDefaultAsync(u => u.UserId == userId);
            string? name = (user != null) ? user.Username : null;
            return name;
        }
        
        public async Task<AppUser?> FindUserByIdAsync(Guid userId) => await _context.AppUsers.SingleOrDefaultAsync(u => u.UserId == userId);
        public async Task<AppUser?> FindUserByUsernameAsync(string username) => await _context.AppUsers.SingleOrDefaultAsync(u => u.Username == username);

        /*------------------END GET-------------------*/

        /*------------------BEGIN CHECK-------------------*/

        //Check if username existed, if existed return true else return false
        public async Task<bool> CheckUsername(string username) => await _context.AppUsers.AnyAsync(a => a.Username == username);

        //Check if email existed, if existed return true else return false
        public async Task<bool> CheckEmail(string email) => await _context.AppUsers.AnyAsync(a => a.Email == email);

        public async Task<bool> IsUserValidated(Guid userId)
        {
            AppUser? user = await _context.AppUsers.SingleOrDefaultAsync(u => u.UserId == userId);
            bool result = (user != null) ? user.IsValidated : false;
            return result;
        }

        public async Task<bool> IsBanned(Guid userId)
        {
                    DateTime? latestUnbannedDate = _context.BanRecords
                    .Where(r => r.TargetUserId == userId)
                    .OrderByDescending(r => r.UnbannedDate)
                    .Select(r => r.UnbannedDate)
                    .FirstOrDefault();

                    if(latestUnbannedDate < DateTime.Now)
                    {
                        AppUser? user = _context.AppUsers.Where(u => u.UserId == userId).SingleOrDefault();
                        if (user != null)
                        {
                            user.IsBanned = false;
                            await _context.SaveChangesAsync();
                        }
                        return false;
                    }
                    return true;
        }

        /*-------------------END CHECK-------------------*/


        /*----------------BEGIN POST----------------------*/

        public async Task<int> CreateAccount(AppUser user)
        {
            if (!_context.AppUsers.Any())
            {
                await _context.AppUsers.AddAsync(user);
            }
            else if (user.Email != null && !(await CheckEmail(user.Email)) && !(await CheckUsername(user.Username)))
            {
                await _context.AppUsers.AddAsync(user);
            }
            return await _context.SaveChangesAsync();
        }


        /*-----------------END POST-------------------*/


        /*---------------BEGIN PUT-------------------*/

        public async Task<int> SetIsAccountValid(bool choice, Guid userId)
        {
              AppUser? user = await _context.AppUsers.SingleOrDefaultAsync(u => u.UserId == userId);
               if (user != null)
               {
                 user.IsValidated = choice;
               }
               return await _context.SaveChangesAsync();
        }

        public async Task<int> SetIsBanned(bool choice, Guid userId)
        {
            AppUser? user = await _context.AppUsers.SingleOrDefaultAsync(u => u.UserId == userId);
            if (user != null)
            {
                user.IsBanned = choice;
            }
            return await _context.SaveChangesAsync();
        }
        /*-----------------END -------------------*/


        /*----------------BEGIN DELETE-----------------------*/

        /*-----------------END DELETE-------------------*/

        /*------------------------------------END APPUSER-------------------------------------------*/
        public async Task<int> UpdateProfileAsync(AppUser user) //SONDB
        {
            try
            {
                using (var _context = new AppDbContext())
                {
                    var existingUser = await _context.AppUsers.FindAsync(user.UserId);
                    if (existingUser != null)
                    {
                        // Update the properties of the existing user with the values of the new user
                        _context.Entry(existingUser).CurrentValues.SetValues(new ProfileUserDTO
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            AvatarDir = user.AvatarDir,
                            Email = user.Email,
                            Phone = user.Phone,
                        });

                        // Save changes to the database asynchronously
                        return await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Handle the case where the user doesn't exist
                        // You might want to throw an exception or return a specific error code
                        return 0; // Indicate that no changes were made
                    }

                }
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"DbUpdateException occurred while updating the user profile: {ex.Message}");
                // You can also log the inner exception if it exists
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
            catch (Exception e)
            {
                // Log any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred while updating the user profile: {e.Message}");
                throw;
            }
        }

        public async Task<string?> GetUserPhoneById(Guid userId) //SONDB
        {
            // Retrieve the user by ID
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserId == userId);

            // Return user's phone if user is found, otherwise return null
            return user?.Phone;
        }
        public async Task<UserPublicProfileDTO> GetUserById(Guid userId) //SONDB
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var user = await context.AppUsers
                                             .FirstOrDefaultAsync(p => p.UserId == userId);

                    if (user != null)
                    {
                        return new UserPublicProfileDTO
                        {
                            Email = user.Email,
                            Username = user.Username,
                            AvatarDir = user.AvatarDir
                        };
                    }
                    else
                    {
                        throw new NullReferenceException($"User with ID '{userId}' not found.");
                    }
                }
            }
            catch (Exception e)
            {
                throw; // Rethrow the original exception
            }
        }

        public async Task<string?> GetUserAvatarDirById(Guid userId) //SONDB
        {
            // Retrieve the user by ID
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserId == userId);

            // Return user's avatar directory if user is found, otherwise return null
            return user?.AvatarDir;
        }
        public string GenerateRandomOTP(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        //public async Task<bool> VerifyEmail(Guid userId)
        //{
        //    // Retrieve the user from the database using the userId
        //    var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserId == userId);

        //    if (user == null)
        //    {
        //        return false; // User not found
        //    }

        //    // Set the IsEmailVerify property to true
        //    user.IsEmailVerify = true;

        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return true; // Operation successful
        //}

        //public async Task<bool> IsEmailVerified(string email)
        //{
        //    // Retrieve the user from the database using the userId
        //    var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

        //    if (user == null)
        //    {
        //        // User not found
        //        return false;
        //    }

        //    // Check if the email is verified
        //    return user.IsEmailVerify;
        //}
    }
}


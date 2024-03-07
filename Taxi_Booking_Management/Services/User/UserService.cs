using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.LoggerService;

namespace Taxi_Booking_Management.Services.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<Models.User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _loggerManager;

        public UserService(UserManager<Models.User> userManager, IMapper mapper, ILoggerManager loggerManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _loggerManager = loggerManager;
        }

        public async Task<Models.User?> GetUserById(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                
                _loggerManager.LogError( $"Error in GetUserById for userId: {userId}");

                throw new ApplicationException($"An error occurred in GetUserById: {ex.Message}", ex);
            }
        }

        public async Task<IdentityResult> UpdateUser(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
               
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _loggerManager.LogInfo($"User with ID {userId} not found.");
                    return IdentityResult.Failed(new IdentityError { Description = "User not found." });
                }

                _mapper.Map(updateUserDto, user);

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _loggerManager.LogInfo($"User with ID {userId} updated successfully.");
                }
                else
                {
                    _loggerManager.LogError($"Failed to update user with ID {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error updating user with ID {userId}: {ex.Message}");
                throw;
            }
        }


    }
}

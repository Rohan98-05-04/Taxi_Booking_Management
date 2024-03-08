using Microsoft.AspNetCore.Identity;
using Taxi_Booking_Management.DtoModels;

namespace Taxi_Booking_Management.Services.User
{
    public interface IUserService
    {
        Task<Models.User?> GetUserById(string userId);
        Task<IdentityResult> UpdateUser(string userId, UpdateUserDto updateUserDto);
    }
}

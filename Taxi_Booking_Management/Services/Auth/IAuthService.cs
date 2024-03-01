using Microsoft.AspNetCore.Identity;
using Taxi_Booking_Management.DtoModels;

namespace Taxi_Booking_Management.Services.Auth
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUser(RegisterUserDto model);

        Task<SignInResult> LogInUser(LogInUserDto model);


        Task SignOut();
    }
}

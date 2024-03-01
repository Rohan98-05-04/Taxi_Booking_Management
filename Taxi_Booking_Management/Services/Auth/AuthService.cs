using Microsoft.AspNetCore.Identity;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Models;

namespace Taxi_Booking_Management.Services.Auth
{
    public class AuthService: IAuthService
    {
        private readonly UserManager<Models.User> _userManager;
        private readonly SignInManager<Models.User> _signInManager;

        public AuthService(UserManager<Models.User> userManager, SignInManager<Models.User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegisterUser(RegisterUserDto model)
        {
            var user = new Models.User()
            {
                Email = model.Email,
                UserName = model.Email,
                Name= model.Name,
                City = model.City,
                State = model.State,
                Country = model.Country,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            return result;
        }

        public async Task<SignInResult> LogInUser(LogInUserDto model)
        {

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            return result;

        }

        public async Task SignOut()
        {
            await _signInManager.SignOutAsync();
        }
    }
}

using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Services.Auth;

namespace Taxi_Booking_Management.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authData;
        private readonly INotyfService _notyf;

        public AuthController(IAuthService authData, INotyfService notyf) 
        {
                _authData=authData;
                _notyf = notyf;

        }
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterUserDto model)
        {
            if (ModelState.IsValid)
            {
                string message = "Fail to signup";
                var result = await _authData.RegisterUser(model);

                if (result == null)
                {
                    message = "Email Already Exist!";
                    return Ok(message);
                }
                else if (!result.Succeeded)
                {
                    return Ok(result.Errors.ToString());
                }
                ModelState.Clear();
                message = "SignUp successfully";
                return Ok("successfully");
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage)
                         .ToList();
            string allErrors = string.Join(", ", errors);
            return Ok(allErrors);
        }

        public IActionResult SignIn()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LogInUserDto model)
        {
            string message = "Fail to login";
            if (ModelState.IsValid)
            {
                var result = await _authData.LogInUser(model);
                if (result.Succeeded)
                {
                    message = "Login successfully";
                    return Ok(message);
                }
              else if (!result.Succeeded)
                {
                    message = "Fail to login, provide valid email/password";
                    return Ok(message);
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage)
                          .ToList();
            string allErrors = string.Join(", ", errors);
            return Ok(allErrors);
        }
        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _authData.SignOut();
           
            return RedirectToRoute(new { controller = "Auth", action = "SignIn" });

        }
    }
}

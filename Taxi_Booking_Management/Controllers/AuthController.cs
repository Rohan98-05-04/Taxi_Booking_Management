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
                var result = await _authData.RegisterUser(model);

                if(result == null)
                {
                    _notyf.Warning("Email Already Exist!");
                }

                else if (!result.Succeeded)
                {
                    foreach (var errormessage in result.Errors)
                    {
                        ModelState.AddModelError("", errormessage.Description);
                        
                    }
                   
                    return View(model);
                }
                ModelState.Clear();

                return Ok("successfully");
            }
            return View(model);
        }

        public IActionResult SignIn()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LogInUserDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authData.LogInUser(model);
                if (result.Succeeded)
                {

                    return Ok("successfully");


                }
                ModelState.AddModelError("", "Invalid Credentials");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _authData.SignOut();
           
            return RedirectToRoute(new { controller = "Auth", action = "SignIn" });

        }
    }
}

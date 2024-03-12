
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Services.Auth;

namespace Taxi_Booking_Management.Controllers
{
    public class AuthController : Controller
    {
        const string SessionName = "_Name";
        private readonly IAuthService _authData;
        private readonly ILoggerManager _loggerManager;

        public AuthController(IAuthService authData, ILoggerManager loggerManager) 
        {
                _authData = authData;
            _loggerManager = loggerManager;
                

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
                    _loggerManager.LogInfo($"Sign up failed: {message}");
                    return Ok(message);
                }
                else if (!result.Succeeded)
                {
                    _loggerManager.LogError($"Sign up failed: {result.Errors}");
                    return Ok(result.Errors.ToString());
                }
                ModelState.Clear();
                message = "SignUp successfully";
                _loggerManager.LogInfo(message);
                return Ok("successfully");
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage)
                         .ToList();
            string allErrors = string.Join(", ", errors);
            _loggerManager.LogError($"Model state is not valid: {allErrors}");
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
                HttpContext.Session.SetString(SessionName, model.Email);
                var result = await _authData.LogInUser(model);
                if (result.Succeeded)
                {
                    message = "Login successfully";
                    _loggerManager.LogInfo(message);
                    return Ok(message);
                }
              else if (!result.Succeeded)
                {
                    message = "Fail to login, provide valid email/password";
                    _loggerManager.LogError(message);
                    return Ok(message);
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage)
                          .ToList();
            string allErrors = string.Join(", ", errors);
            _loggerManager.LogError($"Model state is not valid: {allErrors}");
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

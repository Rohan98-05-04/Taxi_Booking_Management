using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.User;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var SessionAuth = HttpContext.Session.GetString("_Name");
                var user = await _userService.GetUserById(SessionAuth);
                if (user == null)
                {
                    return NotFound($"User with ID not found");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving user details: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> UpdateUser()
        {
            User user = null;
            var SessionAuth = HttpContext.Session.GetString("_Name");
            user = await _userService.GetUserById(SessionAuth);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateUserDto model)
        {
            try
            {
                var result = await _userService.UpdateUser(model.Id, model);

                if (result.Succeeded)
                {
                    return RedirectToAction("GetUserDetails");
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                string allErrors = string.Join(", ", errors);

                return View("Error", model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating user details: {ex.Message}");
            }
        }
    }
}

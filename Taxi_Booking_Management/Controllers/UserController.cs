using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Services.User;

namespace Taxi_Booking_Management.Controllers
{
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
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            try
            {
                var user = await _userService.GetUserById(userId);

                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving user details: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateUser(string userId, UpdateUserDto model)
        {
            try
            {
                var result = await _userService.UpdateUser(userId, model);

                if (result.Succeeded)
                {
                    return RedirectToAction("GetUserDetails", new { userId = userId });
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

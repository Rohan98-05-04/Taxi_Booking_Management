using System.ComponentModel.DataAnnotations;

namespace Taxi_Booking_Management.DtoModels
{
    public class LogInUserDto
    {
        [Required(ErrorMessage = "Please enter your Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email Address")]
        [Display(Name = "Enter your Registered Email")]
        public String Email { get; set; }

        [Required(ErrorMessage = "Please Enter your Password")]
        [Display(Name = "Enter your Password")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}

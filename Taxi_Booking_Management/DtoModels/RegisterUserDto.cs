using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.DtoModels
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Please enter Your Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email Address")]
        [Display(Name = "Email Address")]
        public String Email { get; set; }

        [Required(ErrorMessage = "Please enter a strong Password")]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not Match")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Required(ErrorMessage = "Please Confirm your Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]

        public String ConfirmPassword { get; set; }
        [MaxLength(100)]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(225)")]
        public string? Country { get; set; }
        [Column(TypeName = "varchar(225)")]
        public string? State { get; set; }
        [Column(TypeName = "varchar(225)")]
        public string? City { get; set; }
    }
}

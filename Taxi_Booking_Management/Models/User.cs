using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.Models
{
    public class User : IdentityUser
    {
        [MaxLength(100)]
        [Required(ErrorMessage ="Name is required")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(225)")]
        public string? Country { get; set; }
        [Column(TypeName = "varchar(225)")]
        public string? State { get; set; }
        [Column(TypeName = "varchar(225)")]
        public string? City { get; set; }


    }
}

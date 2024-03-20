using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.Models
{
    public class TaxiDriver
    {
        [Key]
        public int DriverId { get; set; }

        [Required(ErrorMessage ="Driver name is required")]
        [Column(TypeName = "varchar(225)")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "Driver mobile no is required")]
        [Phone]
        [MaxLength(10)]
        [MinLength(10)]
        public string DriverMobile { get; set; }

        [Column(TypeName = "varchar(225)")]
        public string? Address { get; set; }

        [NotMapped]
        public IFormFile? Filename { get; set; }

        public string? FilePath { get; set; }

        public bool IsEnabled { get; set; } = true;

    }
}

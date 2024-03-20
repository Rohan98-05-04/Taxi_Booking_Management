using Microsoft.Build.ObjectModelRemoting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.Models
{
    public class TaxiOwner
    {
        [Key]
        public int  TaxiOwnerId { get; set; }

        [Required(ErrorMessage = "Taxi Owner name is required")]
        [Column(TypeName = "varchar(225)")]
        public string TaxiOwnerName { get; set; }

        [Phone]
        [MaxLength(10)]
        [MinLength(10)]
        [Required(ErrorMessage = "Owner mobile no is required")]
        public string TaxiOwnerMobile { get; set; }

        [EmailAddress]
        public string? TaxiOwnerEmail { get; set; }

        [Column(TypeName = "varchar(225)")]
        public string? TaxiOwnerAddress { get; set; }

        [NotMapped]
        public IFormFile? Filename { get; set; }

        public string? FilePath { get; set; }

        public bool IsEnabled { get; set; } = true;

    }
}

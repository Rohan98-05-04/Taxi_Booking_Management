using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Taxi_Booking_Management.Validations;

namespace Taxi_Booking_Management.DtoModels
{
    public class TaxiViewModel
    {
        public int TaxiViewId { get; set; }
        [Required(ErrorMessage = "taxi name is required")]
        [Display(Name = "Taxi Name")]
        public string TaxiName { get; set; }

        [Required(ErrorMessage = "Registration number is required")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Registration number is required")]
        [Display(Name = "Taxi Type")]
        [AllowNull]
        public int? TaxiType { get; set; } = 0;

        [AllowNull]
        public IEnumerable<SelectListItem?> TaxiTypes { get; set; }

        [Display(Name = "Taxi Status")]
        public int TaxiStatus { get; set; } = 0;

        [AllowNull]
        public IEnumerable<SelectListItem?> TaxiStatuses { get; set; }

        [Display(Name = "Taxi Owner")]
        public int TaxiOwnerId { get; set; } = 0;

        [AllowNull]
        public IEnumerable<SelectListItem?> TaxiOwners { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".pdf", ".jpg", ".jpeg", ".png" }, ErrorMessage = "Only PDF, JPG, JPEG, and PNG files are allowed.")]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = "File size must be less than 1MB.")]
        public IFormFile? Filename { get; set; }

        public string? FilePath { get; set; }

    }
}

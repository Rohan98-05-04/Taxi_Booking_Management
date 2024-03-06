using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Taxi_Booking_Management.DtoModels
{
    public class TaxiViewModel
    {
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

        [Display(Name = "Driver")]
        public int DriverId { get; set; } = 0;

        [AllowNull]
        public IEnumerable<SelectListItem?> Drivers { get; set; }
    }
}

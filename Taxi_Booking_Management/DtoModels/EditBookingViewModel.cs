using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Taxi_Booking_Management.DtoModels
{
    public class EditBookingViewModel
    {

        [Key]
        public int BookingId { get; set; }

        public string? BookingCode { get; set; }

        [Required]


        [DisplayName("Taxi Name")]
        public string RegistrationNo { get; set; }
        public int TaxiId { get; set; } = 0;

        [AllowNull]
        public IEnumerable<SelectListItem?> TaxiNames { get; set; }
        public int DriverId { get; set; } = 0;


        [AllowNull]
        public IEnumerable<SelectListItem?> DriverName { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Column(TypeName = "varchar(225)")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Customer mobile no is required")]
        [Phone]
        public string CustomerMobile { get; set; }

        [Column(TypeName = "decimal(7, 2)")]
        public decimal GrossAmount { get; set; }

        [Column(TypeName = "decimal(7, 2)")]
        [Range(1, 100, ErrorMessage = "Total GST must be between 1 and 100")]
        public decimal TotalGST { get; set; }

        [Column(TypeName = "decimal(7, 2)")]
        public decimal NetAmount { get; set; }

        [Required(ErrorMessage = "From Location is required")]
        [DisplayName("From Location")]
        public string FromLocation { get; set; }

        [Required(ErrorMessage = "To Location is required")]
        [DisplayName("To Location")]
        public string ToLocation { get; set; }

        [Required(ErrorMessage = "Booking status is required")]
        public int BookingStatus { get; set; }

        public DateTime fromDate { get; set; }

        public DateTime toDate { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? UpdatedDateTime { get; set; }



    }
}


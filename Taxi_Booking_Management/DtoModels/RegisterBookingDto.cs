using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Taxi_Booking_Management.Models;
using System.ComponentModel;

namespace Taxi_Booking_Management.DtoModels
{
    public class RegisterBookingDto
    {
            [Key]
            public int BookingId { get; set; }

            public string? BookingCode { get; set; }

            [Required]
            public int TaxiId { get; set; }
            
            public string RegistrationNo { get; set; }

            [Required(ErrorMessage = "Customer name is required")]
            [Column(TypeName = "varchar(225)")]
            public string CustomerName { get; set; }

            [Required(ErrorMessage = "Customer mobile no is required")]
            [Phone]
            public string CustomerMobile { get; set; }

            [Column(TypeName = "decimal(7, 2)")]
            public decimal GrossAmount { get; set; }

            [Column(TypeName = "decimal(7, 2)")]
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

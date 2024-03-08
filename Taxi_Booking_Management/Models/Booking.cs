using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        
        public string BookingCode { get; set; }

        [Required]
        public int TaxiId { get; set; }

        [ForeignKey("TaxiId")]
        public Taxi taxi { get; set; }

        [Required]
        public int DriverId { get; set; }

        [ForeignKey("DriverId")]
        public TaxiDriver TaxiDrivers { get; set; }

        [Required(ErrorMessage ="Customer name is required")]
        [Column(TypeName = "varchar(225)")]
        public string CustomerName {  get; set; }

        [Required(ErrorMessage ="Customer mobile no is required")]
        [Phone]
        public string CustomerMobile { get; set; }

        [Column(TypeName = "decimal(17, 2)")]
        public decimal GrossAmount { get; set; }

        [Column(TypeName = "decimal(17, 2)")]
        public decimal TotalGST { get; set; }

        [Column(TypeName = "decimal(17, 2)")]
        public decimal NetAmount { get; set; }

        [Required(ErrorMessage = "Booking status is required")]
        public int BookingStatus { get; set; }

        [Required(ErrorMessage = "From Location is required")]
        [DisplayName("From Location")]
        public string FromLocation {  get; set; }

        [Required(ErrorMessage = "To Location is required")]
        [DisplayName("To Location")]
        public string ToLocation { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime UpdatedDateTime { get; set;}

    }
}

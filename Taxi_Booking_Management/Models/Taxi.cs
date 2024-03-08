using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.Models
{
    public class Taxi
    {
        [Key]
        public int TaxiId { get; set; }

        [Required(ErrorMessage = "taxi name is required")]
        [Column(TypeName = "varchar(225)")]
        public string TaxiName { get; set; }

        public int? TaxiType { get; set;}

        [Required(ErrorMessage = "Registration number is required")]
        [Column(TypeName = "varchar(125)")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage ="Taxi status is required")]
        public int TaxiStatus {  get; set; }

        [Required]
        public int TaxiOwnerId { get; set; }

        [ForeignKey("TaxiOwnerId")]
        public TaxiOwner TaxiOwner { get; set; }
    }
}

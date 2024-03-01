using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxi_Booking_Management.Models
{
    public class PaymentHistory
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking booking { get; set; }

        public int PaidMedium { get; set; }

        [Column(TypeName = "decimal(7, 2)")]
        public decimal TotalAmount { get; set; }

        public string Remark { get; set; }

        public DateTime createDateTime { get; set; }
        public DateTime updateDateTime { get; set; }
    }
}

namespace Taxi_Booking_Management.DtoModels
{
    public class OneBookingPdfView
    {
        public Models.Booking BookingData { get; set; }
        public IList<Models.PaymentHistory> TransactionData { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
    }
}

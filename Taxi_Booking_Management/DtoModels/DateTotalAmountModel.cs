namespace Taxi_Booking_Management.DtoModels
{
    public class DateTotalAmountModel
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPayAmount { get; set; }
        public decimal PaymentsByCash { get; set; }
        public decimal PaymentsOnline { get; set; }

        public decimal DueAmount { get; set; }
    }
}

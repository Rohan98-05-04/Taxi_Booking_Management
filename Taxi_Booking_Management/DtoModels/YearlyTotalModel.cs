namespace Taxi_Booking_Management.DtoModels
{
    public class YearlyTotalModel
    {
        public int Year {  get; set; }  
        public decimal TotalAmount { get; set; }
        public decimal TotalPayAmount { get; set; }
        public decimal PaymentsByCash { get; set; }
        public decimal PaymentsOnline { get; set; }

        public decimal DueAmount { get; set; }
    }
}

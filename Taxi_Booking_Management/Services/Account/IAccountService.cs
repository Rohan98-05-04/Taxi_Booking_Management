namespace Taxi_Booking_Management.Services.Account
{
    public interface IAccountService
    {
        public decimal GetYearlyTotalBookingAmounts(int year);
        public decimal GetDateBillAmounts(DateTime fromdate, DateTime todate);
        public decimal GetYearlyTotalPaymentHistoryAmounts(int year);
    }
}

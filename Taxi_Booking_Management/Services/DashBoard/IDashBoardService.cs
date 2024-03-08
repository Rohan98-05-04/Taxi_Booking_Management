namespace Taxi_Booking_Management.Services.DashBoard
{
    public interface IDashBoardService
    {
        public Dictionary<string, decimal> GetDailyTotalBillAmounts(DateTime date);
        public List<decimal> GetMonthlyTotalBookingAmounts(int year);
        public Task<int> GetTotalCustomer();
        public Task<int> GetTotalTaxiCount();
        public Task<int> GetTotalTaxiOwner();
        public Task<int> GetTotalDrivers();
        public Task<int> GetTotalBooking();
    }
}

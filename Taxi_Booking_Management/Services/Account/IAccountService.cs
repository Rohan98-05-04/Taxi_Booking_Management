using Taxi_Booking_Management.DtoModels;

namespace Taxi_Booking_Management.Services.Account
{
    public interface IAccountService
    {
        public YearlyTotalModel GetYearlyTotalBookingAmounts(int year);
        public DateTotalAmountModel GetDateBillAmounts(DateTime fromdate, DateTime todate);


        public  Task<string> CreatePdfForAnnualAmount(DtoModels.YearlyTotalModel yearTotalAmountData);

        public Task<string> CreatePdfForDateAmount(DtoModels.DateTotalAmountModel TotalAmountForDate);
    }
}

using X.PagedList;

namespace Taxi_Booking_Management.Services.TaxiDriver
{
    public interface ITaxiDriverService
    {
        public Task<string> RegisterTaxiDriverAsync(Models.TaxiDriver taxiDriver);
        public Task<Models.TaxiDriver?> GetTaxiDriverAsync(int driverId);
        public Task<IPagedList<Models.TaxiDriver>> GetAllTaxiDriverAsync(int page, int pageSize, string search);
        public Task<string> DeleteTaxiDriverAsync(int driverId);
        public Task<string?> UpdateTaxiDriverAsync(Models.TaxiDriver taxiDriver);
        public string GenerateHtmlContentForPdf(IPagedList<Models.TaxiDriver> taxiDrivers);
    }
}

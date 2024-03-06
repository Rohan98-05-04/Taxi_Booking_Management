using Taxi_Booking_Management.DtoModels;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Taxi
{
    public interface ITaxiService
    {
        public Task<string> RegisterTaxiAsync(Models.Taxi taxi);
        public Task<TaxiDto?> GetTaxiDetailsAsync(int taxiId);
        public Task<IPagedList<Models.Taxi>> GetAllTaxiDetailsAsync(int page, int pageSize, string search);
        public Task<string> UpdateTaxiStatus(int taxiId, int taxiStatus);
        public Task<Models.Taxi?> DeleteTaxiAsync(int taxiId);

    }
}

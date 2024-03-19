using X.PagedList;

namespace Taxi_Booking_Management.Services.Customer
{
    public interface ICustomerService
    {
        public Task<IPagedList<Models.Booking>> GetAllCustomerDetailsAsync(int page, int pageSize, string search);
    }
}

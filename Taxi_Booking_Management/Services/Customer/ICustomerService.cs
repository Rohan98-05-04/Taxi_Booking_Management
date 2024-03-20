using Taxi_Booking_Management.DtoModels;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Customer
{
    public interface ICustomerService
    {
        public Task<IPagedList<CustomerViewModel>> GetAllCustomerDetailsAsync(int page, int pageSize, string search);
        public string GenerateHtmlContentForPdf(IPagedList<DtoModels.CustomerViewModel> Customers);
    }
}

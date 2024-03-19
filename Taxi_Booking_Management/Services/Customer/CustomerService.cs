using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using X.PagedList;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Taxi_Booking_Management.Services.Customer
{
    public class CustomerService: ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;

        public CustomerService(ApplicationDbContext context, ILoggerManager loggerManager)
        {
            _context=context;
            _loggerManager = loggerManager;
        }
        public async Task<IPagedList<Models.Booking>> GetAllCustomerDetailsAsync(int page, int pageSize, string search)
        {

            IPagedList<Models.Booking> Customers = null;
            try
            {
                var distinctMobileNumbers = await _context.Bookings
                 .GroupBy(b => b.CustomerMobile)
                 .Select(group => group.FirstOrDefault())
                 .ToListAsync();

                var data = distinctMobileNumbers.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    data = data.Where(b => b.CustomerName.Contains(search) || b.CustomerMobile.Contains(search));
                }
                Customers = await data.ToPagedListAsync(page, pageSize);
                _loggerManager.LogInfo($"all Customer records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllCustomerDetailsAsync");
                throw;
            }
            return Customers;
          

           
        }
            
    }
}

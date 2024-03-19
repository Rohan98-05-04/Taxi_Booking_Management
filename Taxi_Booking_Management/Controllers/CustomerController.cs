using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Customer;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _CustomerService;
        private readonly IConfiguration _configuration;

        public CustomerController(ICustomerService customerService, IConfiguration configuration)
        {
            _CustomerService= customerService;
            _configuration = configuration;
        }
        public async Task<IActionResult> CustomerIndex(int? page,  string search)
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

            IPagedList<Booking> allCustomer;
             allCustomer = await _CustomerService.GetAllCustomerDetailsAsync(pageNumber, pageSize, search);
            return View(allCustomer);
        }
    }
}

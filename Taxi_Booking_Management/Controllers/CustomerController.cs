using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Customer;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _CustomerService;
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _loggerManager;

        public CustomerController(ICustomerService customerService, IConfiguration configuration, ILoggerManager loggerManager)
        {
            _CustomerService= customerService;
            _loggerManager = loggerManager;
            _configuration = configuration;
        }
        public async Task<IActionResult> CustomerIndex(int? page,  string search)
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

            IPagedList<Booking> allCustomer;
             allCustomer = await _CustomerService.GetAllCustomerDetailsAsync(pageNumber, pageSize, search);

            if (Request.Query.ContainsKey("export"))
            {
                var exportType = Request.Query["export"];
                if (exportType == "csv")
                {
                    var propertiesToInclude = new string[] { "CustomerName", "CustomerMobile"};
                    var taxiCustomerList = allCustomer.ToList(); // Convert IPagedList to List
                     var csvData = CsvExportService.GenerateCsvData(taxiCustomerList, propertiesToInclude);

                    // Set the appropriate response headers for CSV download
                    return File(csvData, "text/csv", "taxiCustomer.csv");
                }
            }
            return View(allCustomer);
        }
    }
}

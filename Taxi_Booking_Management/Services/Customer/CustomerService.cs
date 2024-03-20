using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
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
        private readonly IMapper _mapper;

        public CustomerService(ApplicationDbContext context, ILoggerManager loggerManager, IMapper mapper)
        {
            _context=context;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }
        public async Task<IPagedList<CustomerViewModel>> GetAllCustomerDetailsAsync(int page, int pageSize, string search)
        {
            IPagedList<CustomerViewModel> customers = null;
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

                var bookings = await data.ToPagedListAsync(page, pageSize);

                customers = bookings.Select(b => _mapper.Map<CustomerViewModel>(b)).ToPagedList(page, pageSize);

                _loggerManager.LogInfo("All customer records are retrieved");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message}, method name: GetAllCustomerDetailsAsync");
                throw;
            }
            return customers;
        }

        public string GenerateHtmlContentForPdf(IPagedList<DtoModels.CustomerViewModel> Customers)
        {
            // Create an HTML table with student data
            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><head>");
            htmlBuilder.Append("<style>");
            htmlBuilder.Append("table { border-collapse: collapse; width: 100%; border: 1px solid #000; }");
            htmlBuilder.Append("th, td { border: 1px solid #000; padding: 8px; }");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("</head><body>");
            htmlBuilder.Append("<table>");
            htmlBuilder.Append("<thead><tr><th>Customer Name</th><th>Customer Mobile Number</th></tr></thead>");
            htmlBuilder.Append("<tbody>");

            foreach (var customers in Customers)
            {
                htmlBuilder.Append("<tr>");
                htmlBuilder.Append($"<td>{customers.CustomerName}</td>");
                htmlBuilder.Append($"<td>{customers.CustomerMobile}</td>");
                htmlBuilder.Append("</tr>");
            }

            htmlBuilder.Append("</tbody></table>");

            return htmlBuilder.ToString();
        }

    }
}

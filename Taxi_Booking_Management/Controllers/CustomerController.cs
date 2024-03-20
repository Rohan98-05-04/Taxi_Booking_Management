using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.DtoModels;
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
        private readonly IConverter _pdfConverter;

        public CustomerController(ICustomerService customerService, IConfiguration configuration, ILoggerManager loggerManager, IConverter pdfConverter)
        {
            _CustomerService= customerService;
            _loggerManager = loggerManager;
            _configuration = configuration;
            _pdfConverter = pdfConverter;
        }
        public async Task<IActionResult> CustomerIndex(int? page,  string search)
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

            IPagedList<CustomerViewModel> allCustomer;
             allCustomer = await _CustomerService.GetAllCustomerDetailsAsync(pageNumber, pageSize, search);

            if (Request.Query.ContainsKey("export"))
            {
                var exportType = Request.Query["export"];
                if (exportType == "csv")
                {
                    var taxiCustomerList = allCustomer.ToList(); // Convert IPagedList to List
                    var csvData = CsvExportService.GenerateCsvData(taxiCustomerList);

                    // Set the appropriate response headers for CSV download
                    return File(csvData, "text/csv", "taxiCustomer.csv");
                }
                else if (exportType == "pdf")
                {
                    // Generate HTML content for PDF (implement this method)
                    var htmlContent = _CustomerService.GenerateHtmlContentForPdf(allCustomer);

                    // Convert HTML to PDF using DinkToPdf (implement this method)
                    var pdf = _pdfConverter.Convert(new HtmlToPdfDocument
                    {
                        GlobalSettings = new GlobalSettings
                        {
                            // Set global settings (e.g., paper size, margins, etc.)
                            PaperSize = PaperKind.A4,
                            Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                        },
                        Objects = { new ObjectSettings { HtmlContent = htmlContent } }
                    });
                    _loggerManager.LogInfo($"Successfully TaxiOwner PDF File download for {pageNumber}");
                    // Set the appropriate response headers for PDF download
                    return File(pdf, "application/pdf", "CustomerDetails.pdf");
                }
            }
            return View(allCustomer);
        }
    }
}

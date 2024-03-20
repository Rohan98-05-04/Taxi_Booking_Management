using AspNetCoreHero.ToastNotification.Abstractions;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Booking;
using Taxi_Booking_Management.Services.PaymentHistory;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _BookingService;
    
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly IConfiguration _configuration;
        private readonly IConverter _pdfConverter;
        private readonly ILoggerManager _loggerManager;
        const string bookingcontroller = "Booking";
        const string indexAction = "Index";

        public BookingController(IConfiguration configuration,IBookingService context
            , IPaymentHistoryService paymentHistoryService, IConverter pdfConverter, ILoggerManager loggerManager)
        {
            _BookingService=context;
            _paymentHistoryService = paymentHistoryService;
            _configuration = configuration;
            _loggerManager = loggerManager;
            _pdfConverter = pdfConverter;

        }

        public async Task<IActionResult> Index(int? page, string search = "", int? statusFilter = 0, string? startDate = "", string? endDate = "")
        {
            ViewBag.Search = search;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            var pageNumber = page ?? 1;
            int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

            IPagedList<Booking> allBookings;
            allBookings = await _BookingService.GetAllBookingDetailsAsync(pageNumber, pageSize, search, startDate, endDate);
            if (statusFilter > 0)
            {
                if (statusFilter.HasValue)
                {
                    allBookings = allBookings.Where(t => t.BookingStatus == statusFilter).ToPagedList();
                    ViewBag.StatusFilter = statusFilter;
                }
            }
            if (Request.Query.ContainsKey("export"))
            {
                var exportType = Request.Query["export"];
                if (exportType == "csv")
                {

                    var propertiesToInclude = new string[] { "BookingCode", "CustomerName", "CustomerMobile", "GrossAmount", "TotalGST" , "NetAmount"};
                    var taxibookingList = allBookings.ToList(); // Convert IPagedList to List
                    var csvData = CsvExportService.GenerateCsvData(taxibookingList, propertiesToInclude);
                    // Set the appropriate response headers for CSV download
                    return File(csvData, "text/csv", "taxiBookings.csv");
                }
                else if (exportType == "pdf")
                {
                    // Generate HTML content for PDF (implement this method)
                    var htmlContent = _BookingService.GenerateHtmlContentForPdf(allBookings);

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
                    _loggerManager.LogInfo($"Successfully TaxiBookings PDF File download for {pageNumber}");
                    // Set the appropriate response headers for PDF download
                    return File(pdf, "application/pdf", "taxiBookings.pdf");
                }
            }
                return View(allBookings);
        }

        public async Task<IActionResult> RegisterBooking()
        {

            ViewBag.taxiName =  _BookingService.GetTaxiNames();
            ViewBag.driverName =  _BookingService.GetDriverNames();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBooking(RegisterBookingDto dto, [FromServices] INotyfService notyf)
        {
           var data =  await _BookingService.RegisterBookingAsync(dto);
            notyf.Success(data);
            return RedirectToAction(indexAction, bookingcontroller);
        }

        [HttpGet]
        public IActionResult CheckBookingAvailbility()
        {
            CheckTaxiAvailability checkTaxi = new CheckTaxiAvailability();
            ViewBag.taxiName = _BookingService.GetTaxiNames();

            return View(checkTaxi);
        }

        [HttpPost]
        public async Task<IActionResult> CheckBookingAvailbility(CheckTaxiAvailability dto, [FromServices] INotyfService notyf)
        {
            var data = false;
            if (ModelState.IsValid)
            {
               data  = await _BookingService.IsTaxiAvailableAsync(Convert.ToInt32(dto.taxiId), dto.FromDate, dto.ToDate);
              ViewBag.TaxiDates =  await _BookingService.GetTaxiAvailableDates(Convert.ToInt32(dto.taxiId));
            }
            if (data)
            {
                notyf.Success(MessagesAlerts.TaxiIsAvailable);
            }
            else
            {
                notyf.Information(MessagesAlerts.TaxiIsNotAvailable);
            }
            return View(dto);
        }

        [HttpGet]
        public IActionResult GetAvailableTaxi()
        {
            GetAvailableTaxiDto aviTaxi = new GetAvailableTaxiDto();
            return View(aviTaxi);
        }
        [HttpPost]
        public async Task<IActionResult> GetAvailableTaxi(GetAvailableTaxiDto dto, [FromServices] INotyfService notyf)
        {
            if (ModelState.IsValid)
            {
                var data = await _BookingService.GetAvailableTaxisAsync(dto.FromDate, dto.ToDate);
                if (data.Count>0)
                {
                    notyf.Success(MessagesAlerts.TaxiIsAvailable);
                }
                else
                {
                    notyf.Information(MessagesAlerts.TaxiIsNotAvailable);
                }
                ViewBag.TaxiDetails = data;
            }
           
            return View();
        }
        


        [HttpGet]
        public async Task<IActionResult> BookingDetails(int bookingid)
        {
            Booking bookingdetails = null;
            
                bookingdetails = await _BookingService.GetTaxiBookingAsync(bookingid);
                if(bookingdetails != null)
                {
                var transactionsDetails= await _paymentHistoryService.GetPaymentHistoryByBookingId(bookingid);
                ViewBag.TransactionDetails = transactionsDetails;
                    var paidAmount = _paymentHistoryService.GetPaidAmountByBookingId(bookingid);
                    var dueAmount = bookingdetails.NetAmount - paidAmount;
                    ViewBag.DueAmount = dueAmount;
                    ViewBag.PaidAmount = paidAmount;
                if (Request.Query.ContainsKey("export"))
                {
                    var exportType = Request.Query["export"];
                    if (exportType == "pdf")
                    {
                       
                        // Generate HTML content for PDF (implement this method)
                        var htmlContent = _BookingService.CreatePdfForOneBooking(bookingdetails ,transactionsDetails, paidAmount, dueAmount);

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
                        _loggerManager.LogInfo($"Successfully details of TaxiBookings PDF File download for {bookingdetails.BookingCode}");
                        // Set the appropriate response headers for PDF download
                        return File(pdf, "application/pdf", $"taxiBooking({bookingdetails.BookingCode}).pdf");
                    }
                }
                    return View(bookingdetails);
                }
            return RedirectToAction("Error", "Home");
        }
        [HttpGet]

        public async Task<IActionResult> EditBooking(int bookingid, [FromServices] INotyfService notyf)
        {
            if (bookingid > 0)
            {
                var BookingData = await _BookingService.GetBookingDataForUpdate(bookingid);
                if (BookingData != null )
                {
                   
                    return View(BookingData);
                }
                else
                {
                    notyf.Error(MessagesAlerts.BookingDetailsNotAvailable);
                    return RedirectToAction(indexAction, bookingcontroller);
                }

            }
            notyf.Error(MessagesAlerts.InvalidId);
            return RedirectToAction(indexAction, bookingcontroller);

        }
        [HttpPost]
        public async Task<IActionResult> EditBooking(EditBookingViewModel bookingViewModel, [FromServices] INotyfService notyf)
        {
            
            if (bookingViewModel != null)
            {
                var updatedBookingData= await _BookingService.UpdateBookingAsync(bookingViewModel);

                notyf.Success($"{updatedBookingData}");
                return RedirectToAction(indexAction, bookingcontroller);
            }
            notyf.Error($"{MessagesAlerts.FailUpdate}");
            return View(bookingViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, int bookingStatus)
        {
            if (bookingId > 0 && bookingStatus > 0)
            {
                var result = await _BookingService.UpdateBookingStatusById(bookingId, bookingStatus);
                return Json(result);
            }
            return BadRequest();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int bookingId, [FromServices] INotyfService notyf)
        {
            string message = MessagesAlerts.FailDelete;
            if (bookingId > 0)
            {
                var deleteBooking = await _BookingService.DeleteTaxiBookingsAsync(bookingId);
                if (deleteBooking.Contains("successfully"))
                {

                    notyf.Success($"{deleteBooking}");
                    return RedirectToAction(indexAction, bookingcontroller);
                }
                else
                {
                    notyf.Error($"{deleteBooking}");
                }

            }
            notyf.Error($"{message}");
            return RedirectToAction("BookingDetails", bookingcontroller);
        }
    }
}

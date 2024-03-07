using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.PaymentHistory;

namespace Taxi_Booking_Management.Controllers
{
    public class PaymentHistoryController : Controller
    {
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly ApplicationDbContext _context;
        public PaymentHistoryController(IPaymentHistoryService paymentHistoryService, ApplicationDbContext context)
        {
            _paymentHistoryService = paymentHistoryService;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreatePayment()
        {
            ViewBag.Bookings = _context.Bookings.Select(x => new SelectListItem { Value = x.BookingId.ToString(), Text = x.CustomerName });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(PaymentHistory paymentHistory)
        {
            try
            {
                var paymentId = await _paymentHistoryService.CreatePayment(paymentHistory);
                return RedirectToAction("GetAllPayments");
            }
            catch (Exception ex)
            {
                
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments(int? page, string? startDate = "", string? endDate= "")
        {
            try
            {
                ViewBag.startDate = startDate;
                ViewBag.endDate = endDate;
                var pageNumber = page ?? 1;
                var pageSize = 10; 

                var pagedPayments = await _paymentHistoryService.GetAllPayments(pageNumber, pageSize, startDate, endDate);

                return View(pagedPayments);
            }
            catch (Exception ex)
            { 
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetIndividualPaymentByBookingId(int paymentId)
        {
            try
            {
                var payment = await _paymentHistoryService.GetIndividualPaymentById(paymentId);
                if (payment != null)
                {
                    return View(payment);
                }
                else
                {
                    return View("NotFound", paymentId);
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeletePayment(int paymentId, [FromServices] INotyfService notyf)
        {
            if (paymentId > 0)
            {
                var paymentDetails = await _paymentHistoryService.DeletePaymentAsync(paymentId);
                if (paymentDetails != null)
                {
                    notyf.Success("Payment History is delete successfully");
                    return RedirectToAction("GetAllPayments", "PaymentHistory");
                }
                else
                {
                    notyf.Error("Payment not found by given details");
                    return RedirectToAction("GetAllPayments", "PaymentHistory");

                }
            }
            notyf.Error("provide valid paymentId");
            return RedirectToAction("GetAllPayments", "PaymentHistory");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.PaymentHistory;

namespace Taxi_Booking_Management.Controllers
{
    public class PaymentHistoryController : Controller
    {
        private readonly IPaymentHistoryService _paymentHistoryService;
        public PaymentHistoryController(IPaymentHistoryService paymentHistoryService)
        {
            _paymentHistoryService = paymentHistoryService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreatePayment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentHistory paymentHistory)
        {
            try
            {
                var paymentId = await _paymentHistoryService.CreatePayment(paymentHistory);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments(int? page)
        {
            try
            {
                var pageNumber = page ?? 1;
                var pageSize = 10; 

                var pagedPayments = await _paymentHistoryService.GetAllPayments(pageNumber, pageSize);

                return View(pagedPayments);
            }
            catch (Exception ex)
            { 
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetIndividualPaymentByBookingId(int bookingId)
        {
            try
            {
                var payment = await _paymentHistoryService.GetIndividualPaymentByBookingId(bookingId);
                if (payment != null)
                {
                    return View(payment);
                }
                else
                {
                    return View("NotFound", bookingId);
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
    }
}

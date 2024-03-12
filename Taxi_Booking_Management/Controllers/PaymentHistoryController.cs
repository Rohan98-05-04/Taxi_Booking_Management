using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.PaymentHistory;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> CreatePayment(string bookingCode)
        {
            var Bookings = await _context.Bookings.FirstOrDefaultAsync(x => x.BookingCode == bookingCode);
            if(Bookings != null)
            {
                ViewBag.BookingCode = Bookings.BookingCode;
                ViewBag.BookingId = Bookings.BookingId;
               var paidAmount =  _paymentHistoryService.GetPaidAmountByBookingId(Bookings.BookingId);
                var dueAmount = Bookings.NetAmount - paidAmount;
                ViewBag.DueAmount = dueAmount;
                ViewBag.PaidAmount = paidAmount;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(PaymentHistory paymentHistory, [FromServices] INotyfService notyf)
        {
            try
            {
                var paymentId = await _paymentHistoryService.CreatePayment(paymentHistory);
                if(paymentId == null)
                {
                    notyf.Warning("Payment transaction failed");
                    return RedirectToAction("Index","Booking");
                }
                notyf.Success("Payment transaction successfully done");
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

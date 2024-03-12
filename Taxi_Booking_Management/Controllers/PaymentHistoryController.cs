using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Booking;
using Taxi_Booking_Management.Services.PaymentHistory;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class PaymentHistoryController : Controller
    {
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly ILoggerManager _loggerManager;
        private readonly IBookingService _bookingService;

        public PaymentHistoryController(IPaymentHistoryService paymentHistoryService,
              ILoggerManager loggerManager
            ,IBookingService bookingService)
        {
            _paymentHistoryService = paymentHistoryService;
            _loggerManager = loggerManager;
            _bookingService = bookingService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreatePayment(string bookingCode)
        {
            try
            {
                var booking = await _bookingService.GetBookingByBookingCode(bookingCode);
                if (booking != null)
                {
                    ViewBag.BookingCode = booking.BookingCode;
                    ViewBag.BookingId = booking.BookingId;
                    var paidAmount = _paymentHistoryService.GetPaidAmountByBookingId(booking.BookingId);
                    var dueAmount = booking.NetAmount - paidAmount;
                    ViewBag.DueAmount = dueAmount;
                    ViewBag.PaidAmount = paidAmount;
                }
                return View();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while creating payment for booking code {bookingCode}: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
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
                _loggerManager.LogError($"An error occurred while creating payment: {ex.Message}");
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
                _loggerManager.LogError($"An error occurred while fetching all payments: {ex.Message}");
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
                _loggerManager.LogError($"An error occurred while fetching individual payment with ID {paymentId}: {ex.Message}");
                return View("Error");
            }
        }

         

        [HttpPost]
        public async Task<IActionResult> DeletePayment(int paymentId, [FromServices] INotyfService notyf)
        {
            try
            {
                if (paymentId > 0)
                {
                    var paymentDetails = await _paymentHistoryService.DeletePaymentAsync(paymentId);
                    if (paymentDetails != null)
                    {
                        notyf.Success("Payment History is deleted successfully");
                        return RedirectToAction("GetAllPayments", "PaymentHistory");
                    }
                    else
                    {
                        notyf.Error("Payment not found by given details");
                        return RedirectToAction("GetAllPayments", "PaymentHistory");
                    }
                }
                notyf.Error("Provide valid paymentId");
                return RedirectToAction("GetAllPayments", "PaymentHistory");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while deleting payment with ID {paymentId}: {ex.Message}");
                return View("Error");
            }
        }
    }
}

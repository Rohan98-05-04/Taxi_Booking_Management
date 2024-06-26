﻿using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taxi_Booking_Management.DtoModels;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Booking
{
    public interface IBookingService
    {
        public Task<string> RegisterBookingAsync(RegisterBookingDto bookingDto);
        public Task<IPagedList<Models.Booking>> GetAllBookingDetailsAsync(int page, int pageSize, string search, string? startDate, string? endDate);
        public Task<Models.Booking> GetTaxiBookingAsync(int bookingId);
        public Task<List<Models.Taxi>> GetAvailableTaxisAsync(DateTime fromDate, DateTime toDate);
        public Task<bool> IsTaxiAvailableAsync(int taxiId, DateTime fromDate, DateTime toDate);
        public Task<int> GetTaxiIdByRegNo(string regNo);
        public Task<IList<Models.Booking>> GetTaxiAvailableDates(int taxiId);
        public Task<string> UpdateBookingStatusById(int BookingId, int BookingStatus);
        public Task<string> DeleteTaxiBookingsAsync(int bookingId);

        public Task<EditBookingViewModel?> GetBookingDataForUpdate(int bookingid);
        public Task<string> UpdateBookingAsync(EditBookingViewModel bookingmodel);
        public List<SelectListItem> GetTaxiNames();
        public List<SelectListItem> GetDriverNames();

        public Task<Models.Booking> GetBookingByBookingCode(string bookingCode);
        

        public Task<string> GenerateHtmlContentForPdf(IEnumerable<Models.Booking> BookingData);
        

        public Task<string> CreatePdfForOneBooking(Models.Booking bookingData, IList<Models.PaymentHistory> transactionData, decimal paidAmount, decimal dueAmount);
    }
}

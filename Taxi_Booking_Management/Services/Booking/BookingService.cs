using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public BookingService(ApplicationDbContext dbContext, ILoggerManager loggerManager ,  IMapper mapper)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        public async Task<IPagedList<Models.Booking>> GetAllBookingDetailsAsync(int page, int pageSize, string search)
        {
            IPagedList<Models.Booking> Bookings = null;
            try
            {
                IQueryable<Models.Booking> data = _context.Bookings
                .Include(t => t.taxi)
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search) && data != null)
                {
                    data = data.Where(u => u.BookingCode.Contains(search)||u.CustomerName.Contains(search));

                }
                Bookings = await data.ToPagedListAsync(page, pageSize);
                _loggerManager.LogInfo($"all Bookings records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllBookingDetailsAsync");
                throw;
            }
            return Bookings;

        }

        public async Task<BookingDto?> GetTaxiBookingAsync(int bookingId)
        {
            try
            {
                
                var retrieveBooking = await _context.Bookings.Include(t => t.taxi)
                    .FirstOrDefaultAsync(t => t.BookingId == bookingId);
                if (retrieveBooking == null)
                {
                    _loggerManager.LogInfo($"not Booking Details found with bookingId {bookingId}");
                    return null;
                }
                var bookingDto = _mapper.Map<BookingDto>(retrieveBooking);
                _loggerManager.LogInfo($"Booking details is successfully retrived with given id{bookingId}");
                return bookingDto;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTaxiBookingAsync");
                throw;
            }
        }

        public async Task<string> DeleteTaxiBookingsAsync(int bookingId)
        {
            string message = MessagesAlerts.FailDelete;
            try
            {
                var retriveBookingDetails = await _context.Bookings.FirstOrDefaultAsync(u => u.BookingId == bookingId);
                if (retriveBookingDetails == null)
                {
                    _loggerManager.LogInfo($"taxi Booking not found by given id{bookingId}");
                    return message;
                }
                _context.Bookings.Remove(retriveBookingDetails);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullDelete;
                _loggerManager.LogInfo($"taxi Booking is successfully retrived with given id{bookingId}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: DeleteTaxiBookingsAsync");
                throw;
            }
        }

        
    }
}

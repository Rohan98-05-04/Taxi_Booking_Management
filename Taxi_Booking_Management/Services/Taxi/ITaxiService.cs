namespace Taxi_Booking_Management.Services.Taxi
{
    public interface ITaxiService
    {
        public Task<string> RegisterTaxiAsync(Models.Taxi taxi);
         
    }
}

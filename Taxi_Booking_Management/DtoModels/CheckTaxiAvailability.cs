namespace Taxi_Booking_Management.DtoModels
{
    public class CheckTaxiAvailability
    {
        public string taxiId {  get; set; }
        public DateTime FromDate { get; set;}
        public DateTime ToDate { get; set; }
    }
}

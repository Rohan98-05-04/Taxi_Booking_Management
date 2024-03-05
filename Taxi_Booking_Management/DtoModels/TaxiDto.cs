using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Taxi_Booking_Management.DtoModels
{
    public class TaxiDto
    {

        public int TaxiId { get; set; }

        public string TaxiName { get; set; }

        public int? TaxiType { get; set; }

        public string RegistrationNumber { get; set; }

        public int TaxiStatus { get; set; }

        public string DriverName { get; set; }

        public string DriverMobile { get; set; }

        public string TaxiOwnerName { get; set; }

        public string TaxiOwnerMobile { get; set; }

    }
}

namespace Taxi_Booking_Management.Common
{
    public static class Enums
    {
        public enum Roles
        {
            Admin = 1,
        }

        public enum BookingStatus
        {
            Pending = 1,
            Completed = 2,
            Canceled = 3,
        }

        public enum TaxiStatus
        {
            Available = 1,
            Unavailable = 2,
            UnderRepair = 3,
        }

        public enum TaxiType
        {
            Hatchback = 1,
            Sedan = 2,
            SUV = 3,
            MUV =4,
        }

        public enum PaymentMedium
        {
            Cash = 1,
            Online = 2,
        }

        public enum BookingPaymentStatus
        {
            Pending = 1,
            Completed =2
        }


    }
}

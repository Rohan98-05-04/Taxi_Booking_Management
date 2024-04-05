namespace Taxi_Booking_Management.Common
{
    public static class MessagesAlerts
    {
        public static string SuccessfullSave = "Saved successfully";
        public static string FailSave = "Fail to save";
        public static string SuccessfullUpdate = "Updated successfully";
        public static string FailUpdate = "Fail to update";
        public static string SuccessfullDelete = "Deleted successfully";
        public static string FailDelete = "Fail to delete";
        public static string FailToFind = "Unable to find";
        public static string FailInvalidInput = "Invalid input";
        public static string SuccessLogIn = "Login successfully";
        public static string FailLogIn = "Fail to login";
        public static string SuccessSignUp = "SignUP successfully";
        public static string FailSignUp = "Fail to signup";


        //For Bookings

        public static string TaxiIsNotAvailable = "taxi is not available for booking";
        public static string TaxiIsAvailable = "taxi is available for booking";
        public static string BookingDetailsNotAvailable = "Booking details not found by given details";
        public static string InvalidId = "provide valid BookingID";

        //For account report
        public static string NotAvailableAmountDetails = " Amount details Not found ";
        public static string NotaYear = " Please Enter the Specific Year ";
        public static string yearLength = " Year length should be 4 digit ";
    }
}

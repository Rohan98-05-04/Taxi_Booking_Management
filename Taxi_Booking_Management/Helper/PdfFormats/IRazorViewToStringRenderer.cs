namespace Taxi_Booking_Management.Helper.PdfFormats
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
        public byte[] GeneratePdf(string htmlContent);
    }
}

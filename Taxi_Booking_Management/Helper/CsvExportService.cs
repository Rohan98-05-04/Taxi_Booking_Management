using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Globalization;
using Taxi_Booking_Management.Models;
using CsvHelper.Configuration;
using CsvHelper;

namespace Taxi_Booking_Management.Helper
{
    public static class CsvExportService 
    {
        public static byte[] GenerateCsvData<T>(List<T> records)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Get the properties of the generic type T
            var properties = typeof(T).GetProperties();

            // Write header row
            foreach (var property in properties)
            {
                csv.WriteField(property.Name);
            }
            csv.NextRecord();

            // Write records
            foreach (var record in records)
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(record);
                    csv.WriteField(value);
                }
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }
    }
}

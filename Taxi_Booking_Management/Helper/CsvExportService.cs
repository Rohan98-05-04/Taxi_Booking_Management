using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Globalization;
using Taxi_Booking_Management.Models;
using CsvHelper.Configuration;
using CsvHelper;

namespace Taxi_Booking_Management.Helper
{
    public static class CsvExportService 
    {
        public static byte[] GenerateCsvData<T>(List<T> records , string[] propertiesToInclude)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

         
            // Write header row
            foreach (var propertyName in propertiesToInclude)
            {
                csv.WriteField(propertyName);
            }
            csv.NextRecord();

            // Write records
            foreach (var record in records)
            {
                foreach (var propertyName in propertiesToInclude)
                {
                    var property = typeof(T).GetProperty(propertyName);
                    var value = property?.GetValue(record);
                    csv.WriteField(value);
                }
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }
    }
}

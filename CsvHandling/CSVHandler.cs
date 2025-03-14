using CsvHelper.Configuration.Attributes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CsvHelper;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;

namespace ITEQ2.CsvHandling
{
    public class CSVHandler // Class for CSV files to be made into lists
    {
        private readonly string filePath;

        public CSVHandler(Path path)
        {
            this.filePath = path.FilePath;
        }

        public List<T> ReadFile<T>(Path path) where T : class 
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,     // Ignore header validation errors
                MissingFieldFound = null,   // Ignore missing fields errors
                Delimiter = ",",            // Ensure correct CSV delimiter
                BadDataFound = null         // Ignore bad data instead of throwing exceptions
            };

            try
            {
                using var reader = new StreamReader(path.FilePath); // Read 
                using var csv = new CsvReader(reader, config);

                // Read the header to detect the file type
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                // Determine model based on headers
                bool isFucModel = headers.Contains("PC") && headers.Contains("Username");
                bool isIteqModel = headers.Contains("GG-LABEL") && headers.Contains("User");

                if (isFucModel && typeof(T) == typeof(FucModel))
                {
                    System.Diagnostics.Debug.WriteLine("Detected FucModel structure. Reading file...");
                    return csv.GetRecords<T>().ToList();
                }
                else if (isIteqModel && typeof(T) == typeof(ITEQModel))
                {
                    System.Diagnostics.Debug.WriteLine("Detected ITEQModel structure. Reading file...");
                    return csv.GetRecords<T>().ToList();
                }
                else
                {
                    throw new Exception("Unknown CSV format. Cannot determine model.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading CSV file: {ex.Message}");
                return new List<T>(); // Make empty list
            }
        }
    }
}

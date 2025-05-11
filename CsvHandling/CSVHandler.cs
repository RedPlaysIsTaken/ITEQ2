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
                Delimiter = ",",
                Quote = '"',
                TrimOptions = TrimOptions.None,
                IgnoreBlankLines = true,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            try
            {
                using var reader = new StreamReader(path.FilePath);
                using var csv = new CsvReader(reader, config);

                // detect the headers to detect the file type*
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                // choose model based on headers
                bool isFucModel = headers.Intersect(new[] { "PC", "Username" }).Count() == 2;
                bool isIteqModel = headers.Intersect(new[] { "GG-LABEL", "User" }).Count() == 2;

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

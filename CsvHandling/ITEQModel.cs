using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CsvHelper.TypeConversion;
using DateTimeConverter = CsvHelper.TypeConversion.DateTimeConverter;
using TypeConverterAttribute = CsvHelper.Configuration.Attributes.TypeConverterAttribute;

namespace ITEQ2.CsvHandling
{
    public class ITEQModel
    {
        [Name("Column")]
        public string Column { get; set; }

        [Name("GG-LABEL")]
        public string GgLabel { get; set; }

        [Name("TYPE")]
        public string Type { get; set; }

        [Name("MAKE")]
        public string Make { get; set; }

        [Name("MODEL")]
        public string Model { get; set; }

        [Name("SERIAL NO")]
        public string SerialNo { get; set; }

        [Name("SECURITY ID")]
        public string SecurityId { get; set; }

        [Name("User")]
        public string User { get; set; }

        [Name("Site")]
        public string Site { get; set; }

        [Name("Status")]
        public string Status { get; set; }

        [Name("Purchase date")]
        [TypeConverter(typeof(CustomDateConverter))] // Use custom date converter
        public DateTime? PurchaseDate { get; set; }

        [Name("Recieved")]
        [TypeConverter(typeof(CustomDateConverter))] // Use custom date converter
        public DateTime? Received { get; set; }

        [Name("Short comment")]
        public string ShortComment { get; set; }
    }

    // Custom converter to handle different date formats
    public class CustomDateConverter : DateTimeConverter
    {
        private static readonly string[] DateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "yyyy/MM/dd" };

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null; // Handle empty date fields

            if (DateTime.TryParseExact(text, DateFormats, System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            return base.ConvertFromString(text, row, memberMapData); // Fallback
        }
    }
}

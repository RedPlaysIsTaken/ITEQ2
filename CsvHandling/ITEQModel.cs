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
        [TypeConverter(typeof(CustomDateConverter))]
        public DateTime? PurchaseDate { get; set; }

        [Name("Recieved")]
        [TypeConverter(typeof(CustomDateConverter))]
        public DateTime? Received { get; set; }

        [Name("Short comment")]
        public string ShortComment { get; set; }
    }
}

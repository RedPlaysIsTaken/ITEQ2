using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.TypeConversion;

namespace ITEQ2.CsvHandling
{
    public class CustomDateConverter : DateTimeConverter // Converts date to correct format
    {
        private static readonly string[] DateFormats =
            {
            "dd.MM.yyyy",
            "MM/dd/yyyy",
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "yyyy/MM/dd",
            "yyyy/MM/dd HH:mm"
            };

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (DateTime.TryParseExact(text, DateFormats, System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}

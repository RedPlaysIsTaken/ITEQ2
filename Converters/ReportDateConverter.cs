using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ITEQ2.Logic;

namespace ITEQ2.Converters
{
    class ReportDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime reportDate)
            {
                var daysOld = DateChecker.GetDaysSinceReport(reportDate);
                return daysOld >= 90;
            }
            return false; // Treat null/non-DateTime as "not old"
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
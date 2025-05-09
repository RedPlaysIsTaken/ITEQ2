using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITEQ2.Logic
{
    public static class DateChecker
    {
        public static int GetDaysSinceReport(DateTime reportDate)
        {
            DateTime today = DateTime.Now.Date;
            DateTime report = reportDate.Date;
            return (today - report).Days;
        }
    }
}

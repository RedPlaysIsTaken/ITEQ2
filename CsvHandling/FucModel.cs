using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace ITEQ2.CsvHandling
{
    public class FucModel
    {
        [Name("PC")]
        public string PC { get; set; }

        [Name("USER")]
        public string USER { get; set; }

        [Name("Username")]
        public string Username { get; set; }

        [Name("DATE")]
        public DateTime DATE { get; set; }

        [Name("ReportDATE")]
        public DateTime ReportDATE { get; set; }

        [Name("PCLocation")]
        public string PCLocation { get; set; }

        [Name("Empl_Mailadresse")]
        public string Empl_Mailadresse { get; set; }
    }
}

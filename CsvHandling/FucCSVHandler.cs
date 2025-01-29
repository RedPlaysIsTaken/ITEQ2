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

namespace ITEQ2.CsvHandling
{
    public class FucCSVHandler
    {
        private string filePath;

        public FucCSVHandler(FucPath fucPath)
        {
            this.filePath = fucPath.FilePath;
        }

        public void ReadFile()
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader,CultureInfo.InvariantCulture);
        }
    }
}

//namespace ITEQ2.CsvHandling
//{

//    class FucCSVHandler
//    {
//        [Name("PC")]
//        public string PC { get; set; }

//        [Name("USER")]
//        public string USER { get; set; }

//        [Name("Username")]
//        public string Username { get; set; }

//        [Name("DATE")]
//        public DateTime DATE { get; set; }

//        [Name("ReportDATE")]
//        public DateTime ReportDATE { get; set; }

//        [Name("PCLocation")]
//        public string PCLocation { get; set; }

//        [Name("Empl_Mailadresse")]
//        public string Empl_Mailadresse { get; set; }
//    }

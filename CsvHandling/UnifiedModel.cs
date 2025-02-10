using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITEQ2.CsvHandling
{
    public class UnifiedModel
    {
        public string GgLabel { get; set; }
        public string User { get; set; }
        public string Type { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string SerialNo { get; set; }
        public string SecurityId { get; set; }
        public string Site { get; set; }
        public string Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? Received { get; set; }
        public string ShortComment { get; set; }
        public string PC { get; set; }
        public string Username { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ReportDate { get; set; }
        public string PCLocation { get; set; }
        public string EmplMailAdresse { get; set; }
    }
}

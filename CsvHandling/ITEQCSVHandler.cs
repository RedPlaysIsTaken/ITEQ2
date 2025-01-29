using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITEQ2.CsvHandling
{
    public class ITEQCSVHandler
    {
        private string filePath;

        public ITEQCSVHandler(ITEQPath iteqPath)
        {
            this.filePath = iteqPath.FilePath;
        }

        // Additional methods to handle CSV operations can be added here
    }
}

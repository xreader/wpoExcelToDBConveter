using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Services;

namespace RemehaClassLibrary.Services
{
    public class PumpServiceRemeha : PumpService
    {
        private readonly XLWorkbook workbook;
        public record Cell(string Letter, int Num, string Data);

        public PumpServiceRemeha(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
    }
}

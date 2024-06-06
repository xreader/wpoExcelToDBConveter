using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClassLibrary.Models
{
    public class UnregulatedPump
    {
        public string Name { get; set; }
        public Dictionary<int, List<UnregulatedDataPump>> Data { get; set; }

        protected IXLWorksheet _sheet;
        public UnregulatedPump(IXLWorksheet sheet)
        {
            _sheet = sheet;
        }
        public UnregulatedPump()
        {

        }

     
    }
}

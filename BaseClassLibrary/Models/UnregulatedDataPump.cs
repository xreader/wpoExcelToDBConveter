using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClassLibrary.Models
{
    public class UnregulatedDataPump
    {
        public int Temp { get; set; }
        public int MaxVorlauftemperatur { get; set; }
        public double HC { get; set; }
        public double COP { get; set; }
    }
}

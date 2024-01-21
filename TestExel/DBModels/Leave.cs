using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.DBModels
{
    public class Leave
    {
        public int objectid_fk_properties_objectid { get; set; }
        public int nodeid_fk_nodes_nodeid { get; set; }
        public string value { get; set; }
        public int? value_as_int { get; set; }
    }
}

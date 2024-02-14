using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.DBModels
{
    public class Node
    {
        public int nodeid { get; set; }
        public int typeid_fk_types_typeid { get; set; }
        public int parentid_fk_nodes_nodeid { get; set; }
        public int licence { get;set; }

    }
}

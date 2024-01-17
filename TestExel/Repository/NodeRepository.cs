using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;

namespace TestExel.Repository
{
    public class NodeRepository
    {
        private readonly ApplicationDBContext _context;
        public NodeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public List<int> GetIdLeavesWithDataByPumpId(int pumpId) => _context.nodes.Where(x=> x.parentid_fk_nodes_nodeid == pumpId && x.typeid_fk_types_typeid == 25).OrderBy(x=>x.nodeid).Select(x=>x.nodeid).ToList();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;

namespace TestExel.Repository
{
    internal class PumpRepositoryForDB
    {
        private readonly ApplicationDBContext _context;
        public PumpRepositoryForDB(ApplicationDBContext context)
        {
            _context = context;
        }

        public List<Leaves> FindLeaveByNamePump(string name) => _context.leaves.Where(x => x.value.Contains(name)).ToList();
        public int GetCountLeavesById(int id) => _context.leaves.Count(x => x.nodeid_fk_nodes_nodeid == id);  
        public List<Leaves> GetLeavesById(int id) => _context.leaves.Where(x => x.nodeid_fk_nodes_nodeid == id).ToList();

        public Leaves GetBigHashFor35GradForKaltesKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1464 && x.nodeid_fk_nodes_nodeid == wpId);
        public Leaves GetBigHashFor55GradForKaltesKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1466 && x.nodeid_fk_nodes_nodeid == wpId);
        public Leaves GetBigHashFor35GradForMittelKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1364 && x.nodeid_fk_nodes_nodeid == wpId);
        public Leaves GetBigHashFor55GradForMittelKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1366 && x.nodeid_fk_nodes_nodeid == wpId);

        public bool UpdateLeaves(Leaves leaves)
        {
            _context.Update(leaves);
            return SaveContext();
        }
        public bool SaveContext()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;

namespace TestExel.Repository
{
    internal class LeaveRepository
    {
        private readonly ApplicationDBContext _context;
        public LeaveRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public List<Leave> FindLeaveByNamePump(string pumpName) => _context.leaves.Where(x => x.value.Contains(pumpName) && x.objectid_fk_properties_objectid== 1320).ToList();
        public int GetCountLeavesById(int id) => _context.leaves.Count(x => x.nodeid_fk_nodes_nodeid == id);  
        public List<Leave> GetLeavesById(int id) => _context.leaves.Where(x => x.nodeid_fk_nodes_nodeid == id).ToList();

        public Leave GetBigHashFor35GradForKaltesKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1464 && x.nodeid_fk_nodes_nodeid == wpId);
        public Leave GetBigHashFor55GradForKaltesKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1466 && x.nodeid_fk_nodes_nodeid == wpId);
        public Leave GetBigHashFor35GradForMittelKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1364 && x.nodeid_fk_nodes_nodeid == wpId);
        public Leave GetBigHashFor55GradForMittelKlimaByWpId(int wpId) => _context.leaves.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1366 && x.nodeid_fk_nodes_nodeid == wpId);

        public bool UpdateLeaves(Leave leaves)
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

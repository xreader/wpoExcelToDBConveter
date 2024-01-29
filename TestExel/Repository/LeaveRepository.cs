using Microsoft.EntityFrameworkCore;
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
        public async Task<List<List<Leave>>> GetLeavesByIdList(List<int> ids)
        {
            var result = new List<List<Leave>>();

            foreach (var id in ids)
            {
                var leaves = await _context.leaves.Where(x => x.nodeid_fk_nodes_nodeid == id).ToListAsync();
                result.Add(leaves);
            }

            return result;
        }
        public async Task<List<Leave>> FindLeaveByNamePump(string pumpName) => await _context.leaves.Where(x => x.value.Contains(pumpName) && x.objectid_fk_properties_objectid== 1320).ToListAsync();
        public async Task<int> GetCountLeavesById(int id) => await _context.leaves.CountAsync(x => x.nodeid_fk_nodes_nodeid == id);  
        public async Task<List<Leave>> GetLeavesById(int id) => await _context.leaves.Where(x => x.nodeid_fk_nodes_nodeid == id).ToListAsync();

        public async Task<Leave> GetBigHashFor35GradForKaltesKlimaByWpId(int wpId) => await _context.leaves.FirstOrDefaultAsync(x => x.objectid_fk_properties_objectid == 1464 && x.nodeid_fk_nodes_nodeid == wpId);
        public async Task<Leave> GetBigHashFor55GradForKaltesKlimaByWpId(int wpId) => await _context.leaves.FirstOrDefaultAsync(x => x.objectid_fk_properties_objectid == 1466 && x.nodeid_fk_nodes_nodeid == wpId);
        public async Task<Leave> GetBigHashFor35GradForMittelKlimaByWpId(int wpId) => await _context.leaves.FirstOrDefaultAsync(x => x.objectid_fk_properties_objectid == 1364 && x.nodeid_fk_nodes_nodeid == wpId);
        public async Task<Leave> GetBigHashFor55GradForMittelKlimaByWpId(int wpId) => await _context.leaves.FirstOrDefaultAsync(x => x.objectid_fk_properties_objectid == 1366 && x.nodeid_fk_nodes_nodeid == wpId);        
        
        public async Task<bool> CreateLeave(Leave leave)
        {
            await _context.leaves.AddAsync(leave);
            return await SaveAsync();
        }
        public async Task<bool> DeleteLeaves(List<List<Leave>> leaves)
        {
            foreach(var listLeaves in leaves)
            {
                foreach (Leave leave in listLeaves)
                {
                    _context.leaves.Remove(leave);
                }

            }            
            return await SaveAsync();
        }
        public async Task<bool> UpdateLeaves(Leave leaves)
        {
            _context.Update(leaves);
            return await SaveAsync();
        }
        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0 ? true : false;
        }
    }
}

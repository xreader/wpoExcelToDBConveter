﻿using Microsoft.EntityFrameworkCore;
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
        //Get leaves id for Data EN 14825 LG
        public async Task<List<int>> GetIdLeavesWithDataByPumpId(int pumpId) => await _context.nodes.Where(x=> x.parentid_fk_nodes_nodeid == pumpId && x.typeid_fk_types_typeid == 25)
                                                                                  .OrderBy(x=>x.nodeid)
                                                                                  .Select(x=>x.nodeid)
                                                                                  .ToListAsync();
        //Get leaves id for Leistungsdaten
        public async Task<List<int>> GetIdLeavesWithLeistungsdatenByPumpId(int pumpId) => await _context.nodes.Where(x => x.parentid_fk_nodes_nodeid == pumpId && x.typeid_fk_types_typeid == 8)
                                                                                  .OrderBy(x => x.nodeid)
                                                                                  .Select(x => x.nodeid)
                                                                                  .ToListAsync();
        public async Task<Node> GetNodeByIdAsync(int idNode) => await _context.nodes.FirstOrDefaultAsync(x => x.nodeid == idNode);
        public async Task<bool> DeleteNode(Node node)
        {
            _context.nodes.Remove(node);
            return await SaveAsync();
        }
        public async Task<bool> CreateNode(Node node)
        {
            await _context.nodes.AddAsync(node);
            return await SaveAsync();
        }
        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0 ? true : false;
        }
    }
}

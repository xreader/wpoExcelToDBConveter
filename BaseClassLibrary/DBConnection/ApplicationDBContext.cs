using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using System.Threading.Tasks;
using TestExel.DBModels;

namespace TestExel.DBConnection
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        
        public DbSet<Leave> leaves { get; set; }
        public DbSet<Node>  nodes { get; set; }
        public DbSet<Text> texts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Leave>().HasKey(l => new { l.objectid_fk_properties_objectid, l.nodeid_fk_nodes_nodeid });
            modelBuilder.Entity<Text>().HasKey(l => new { l.textid, l.version});

            base.OnModelCreating(modelBuilder);
        }
    }
}

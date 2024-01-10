using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;

namespace TestExel
{
    internal class Repo
    {
        private readonly ApplicationDBContext _context;

        public Repo(ApplicationDBContext context)
        {
            _context = context;
        }

        
    }
}

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

        public void GetTest()
        {
            var a = _context.texts.Where(x=> x.textId <10).ToList();
            foreach (var x in a)
            {
                Console.WriteLine(x.ger);
            }
        }
    }
}

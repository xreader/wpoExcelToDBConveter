using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.Repository;
using TestExel.ServicesForDB;

namespace HovalClassLibrary.DBService
{
    internal class PumpServiceForDBHoval : PumpServiceForDB
    {
        private readonly LeaveRepository _leaveRepository;
        private readonly NodeRepository _nodeRepository;
        private readonly TextRepository _textRepository;
        public PumpServiceForDBHoval(string pathDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
               .UseSqlite("Data Source=" + pathDB + ";")
               .Options;
            _leaveRepository = new LeaveRepository(new ApplicationDBContext(options));
            _nodeRepository = new NodeRepository(new ApplicationDBContext(options));
            _textRepository = new TextRepository(new ApplicationDBContext(options));
        }
    }
}

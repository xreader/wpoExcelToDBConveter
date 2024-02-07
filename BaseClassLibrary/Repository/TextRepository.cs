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
    public class TextRepository
    {
        private readonly ApplicationDBContext _context;
        public TextRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public List<Text> GetAllTexts()
        {
            return _context.texts.ToList();
        }
        public async Task<List<Text>> FindTextIdByGerName(string gerName)
        {
            var text = await _context.texts.Where(x => x.ger == gerName || x.ger.Contains(gerName+"+")).ToListAsync();            
            return text;
        }

    }
}

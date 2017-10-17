using System.Linq;
using System.Threading.Tasks;
using LiveBolt.Models;
using Microsoft.EntityFrameworkCore;

namespace LiveBolt.Data
{
    public class Repository : IRepository
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Home> GetHomeById(int? id)
        {
            return id == null ? null : await _context.Homes.FindAsync(id);
        }

        public async Task<Home> AddHome(Home home)
        {
            await _context.Homes.AddAsync(home);

            await Commit();

            return await _context.Homes.Where(h => h.Name == home.Name && h.PasswordHash == home.PasswordHash).FirstAsync();
        }

        public void RemoveHome(Home home)
        {
            _context.Homes.Remove(home);
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }
    }
}
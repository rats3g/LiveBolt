using System;
using System.Linq;
using System.Security.Cryptography;
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
            return id == null ? null : await _context.Homes.Where(home => home.Id == id).Include(home => home.Users).FirstOrDefaultAsync();
        }

        public Home GetHomeByNameAndPassword(string name, string password)
        {
            var matchingNames = _context.Homes.Where(home => home.Name == name).Include(home => home.Users);

            foreach (var matchName in matchingNames)
            {
                var givenPasswordHash = new Rfc2898DeriveBytes(password, matchName.Salt).GetBytes(256);

                if (matchName.PasswordHash.SequenceEqual(givenPasswordHash))
                {
                    return matchName;
                }
            }

            return null;
        }

        public bool ContainsHome(string name)
        {
            return _context.Homes.Any(home => home.Name == name);
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
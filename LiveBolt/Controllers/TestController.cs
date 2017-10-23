
using System.Threading.Tasks;
using LiveBolt.Data;
using Microsoft.AspNetCore.Mvc;

namespace LiveBolt.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpDelete]
        public async Task<IActionResult> ClearDatabase()
        {
            _context.Homes.RemoveRange(_context.Homes);
            _context.Users.RemoveRange(_context.Users);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}

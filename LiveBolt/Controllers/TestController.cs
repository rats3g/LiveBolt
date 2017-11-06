
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.Extensions;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.HomeViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LiveBolt.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public TestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpDelete]
        public async Task<IActionResult> ClearDatabase()
        {
            _context.DLMs.RemoveRange(_context.DLMs);
            _context.IDMs.RemoveRange(_context.IDMs);
            _context.Homes.RemoveRange(_context.Homes);
            _context.Users.RemoveRange(_context.Users);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SeedDatabase()
        {
            if (!_context.Users.IsNullOrEmpty() || !_context.Homes.IsNullOrEmpty() || !_context.DLMs.IsNullOrEmpty() || !_context.IDMs.IsNullOrEmpty())
            {
                return BadRequest("Database must be empty.");
            }

            var user1 = new ApplicationUser { UserName = "rat56@pitt.edu", Email = "rat56@pitt.edu", FirstName = "Robert", LastName = "Taylor", IsHome = true};
            var user2 = new ApplicationUser { UserName = "rjb82@pitt.edu", Email = "rjb82@pitt.edu", FirstName = "Ryan", LastName = "Becker", IsHome = false};
            await _userManager.CreateAsync(user1, "Testing123!");
            await _userManager.CreateAsync(user2, "Testing123!");

            var dlm1 = new DLM
            {
                Id = Guid.NewGuid(),
                Nickname = "Front Door",
                IsLocked = true,
                AssociatedHomeId = 1
            };

            var dlm2 = new DLM
            {
                Id = Guid.NewGuid(),
                Nickname = "Back Door",
                IsLocked = false,
                AssociatedHomeId = 1
            };

            await _context.DLMs.AddAsync(dlm1);
            await _context.DLMs.AddAsync(dlm2);

            var idm1 = new IDM
            {
                Id = Guid.NewGuid(),
                Nickname = "Bedroom Window",
                IsClosed = true,
                AssociatedHomeId = 1
            };

            var idm2 = new IDM
            {
                Id = Guid.NewGuid(),
                Nickname = "Kitchen Window",
                IsClosed = false,
                AssociatedHomeId = 1
            };

            await _context.IDMs.AddAsync(idm1);
            await _context.IDMs.AddAsync(idm2);

            var passwordHasher = new Rfc2898DeriveBytes("Testing123!", 256);

            var home = new Home
            {
                Id = 1,
                Name = "TestHome",
                Nickname = "Test Home",
                PasswordHash = passwordHasher.GetBytes(256),
                Salt = passwordHasher.Salt,
                Latitude = 40.4378560,
                Longitude = -79.9625200,
                Users = new List<ApplicationUser>
                {
                    user1,
                    user2
                },
                DLMs = new List<DLM>
                {
                    dlm1,
                    dlm2
                },
                IDMs = new List<IDM>
                {
                    idm1,
                    idm2
                }
            };

            await _context.Homes.AddAsync(home);

            user1.HomeId = 1;
            user2.HomeId = 1;

            await _context.SaveChangesAsync();

            var storedHome = await _context.Homes.FindAsync(1);
            var mappedHome = _mapper.Map<Home, HomeStatusViewModel>(storedHome);

            return Ok(mappedHome);
        }
    }
}

﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.Extensions;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.HomeViewModels;
using LiveBolt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace LiveBolt.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IAPNSService _apns;
        private readonly IRepository _repository;
        private readonly IMLService _mLService;

        public TestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper, IAPNSService apns, IRepository repository, IMLService mLService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _apns = apns;
            _repository = repository;
            _mLService = mLService;
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
                Id = new Guid("66155e63-3b3d-41fc-a3ff-34cd7b521a59"),
                Nickname = "Front Door",
                IsLocked = true,
                AssociatedHomeId = 1
            };

            var dlm2 = new DLM
            {
                Id = new Guid("229371a2-0af0-4514-ade7-ad339f94ced4"),
                Nickname = "Back Door",
                IsLocked = false,
                AssociatedHomeId = 1
            };

            await _context.DLMs.AddAsync(dlm1);
            await _context.DLMs.AddAsync(dlm2);

            var idm1 = new IDM
            {
                Id = new Guid("e9c7aa44-0653-4f5e-b5b7-0aa7c2fdeea6"),
                Nickname = "Bedroom Window",
                IsClosed = true,
                AssociatedHomeId = 1
            };

            var idm2 = new IDM
            {
                Id = new Guid("cc46356a-2ea8-4fd9-b4d2-1305c974c788"),
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
                Latitude = 40.759211,
                Longitude = -73.984638,
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendPushForHome()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null)
            {
                return BadRequest();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            _apns.SendPushNotifications(home.Users.Where(user => user.DeviceToken != null && user.DeviceToken.Length == 64).Select(user => user.DeviceToken), JObject.Parse("{'aps':{'alert':{'title': 'Home Alert','body': 'Home is in an unsafe state. Would you like to lock your doors?'},'badge':1,'sound':'default','category': 'ML_CATEGORY'}}"));

            return Ok(home.Users.Where(user => user.DeviceToken != null && user.DeviceToken.Length == 64).Select(user => user.DeviceToken));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> TestMachineLearning()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null)
            {
                return BadRequest();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            _mLService.checkHomeStatus(home);

            return Ok();
        }
    }
}

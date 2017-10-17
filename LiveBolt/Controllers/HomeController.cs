using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.HomeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LiveBolt.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository _repository;

        public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, IRepository repository)
        {
            _userManager = userManager;
            _logger = logger;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId != null)
            {
                return BadRequest();
            }

            var passwordHasher = new Rfc2898DeriveBytes(model.Password, 256);

            var home = new Home()
            {
                Name = model.Name,
                Salt = passwordHasher.Salt,
                PasswordHash = passwordHasher.GetBytes(256),
                Users = new List<ApplicationUser>
                {
                    currentUser
                }
            };

            var newHome = await _repository.AddHome(home);

            currentUser.HomeId = newHome.Id;

            await _repository.Commit();

            return Ok(newHome); // TODO: Map this to a view model in order to not expose ID and all User information
        }

        [HttpGet]
        public async Task<IActionResult> Status()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null)
            {
                return NotFound();
            }

            var home = _repository.GetHomeById(currentUser.HomeId);

            return Ok(home); // TODO: Map this to a view model in order to not expose ID, PASSWORD, and all User information
        }

        public async Task<IActionResult> Remove()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null)
            {
                return Ok();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            _repository.RemoveHome(home);

            await _repository.Commit();

            return Ok();
        }
    }
}
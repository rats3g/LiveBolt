using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.HomeViewModels;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IMapper _mapper;

        public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, IRepository repository, IMapper mapper)
        {
            _userManager = userManager;
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
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

                var mappedHome = _mapper.Map<Home, HomeStatusViewModel>(newHome);

                return Ok(mappedHome); // TODO: Map this to a view model in order to not expose ID and all User information
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> Status()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null)
            {
                return NotFound();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            var mappedHome = _mapper.Map<Home, HomeStatusViewModel>(home);

            return Ok(mappedHome); // TODO: Map this to a view model in order to not expose ID, PASSWORD, and all User information
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
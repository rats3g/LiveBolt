using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.HomeViewModels;
using LiveBolt.Services;
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
        private readonly IMqttService _mqtt;

        public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, IRepository repository, IMapper mapper, IMqttService mqtt)
        {
            _userManager = userManager;
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _mqtt = mqtt;
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

                var passwordHasher = new Rfc2898DeriveBytes(model.Password, 256); // Should be using larger iteration count

                if (_repository.ContainsHome(model.Name))
                {
                    ModelState.AddModelError("ErrorMessage", "Home with that name already exists");
                    return BadRequest(ModelState);
                }

                var home = new Home
                {
                    Name = model.Name,
                    Nickname = model.Nickname,
                    Salt = passwordHasher.Salt,
                    PasswordHash = passwordHasher.GetBytes(256),
                    Longitude = model.Longitude,
                    Latitude = model.Latitude,
                    Users = new List<ApplicationUser>
                    {
                        currentUser
                    }
                };

                var newHome = await _repository.AddHome(home);

                currentUser.HomeId = newHome.Id;
                currentUser.IsHome = true;

                await _repository.Commit();

                var mappedHome = _mapper.Map<Home, HomeStatusViewModel>(newHome);

                return Ok(mappedHome);
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

            return Ok(mappedHome);
        }

        [HttpPost]
        public async Task<IActionResult> Join(JoinViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);

                if (currentUser.HomeId != null)
                {
                    return BadRequest();
                }

                var joinHome = _repository.GetHomeByNameAndPassword(model.Name, model.Password);

                if (joinHome == null)
                {
                    ModelState.AddModelError("ErrorMessage", "Invalid home login.");
                    return BadRequest(ModelState);
                }

                joinHome.Users.Add(currentUser);
                currentUser.HomeId = joinHome.Id;

                await _repository.Commit();

                var mappedHome = _mapper.Map<Home, HomeStatusViewModel>(joinHome);

                return Ok(mappedHome);
            }

            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Remove()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null)
            {
                return Ok();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            home.Users.Remove(currentUser);
            currentUser.HomeId = null;

            await _repository.Commit();

            if (home.Users.Count == 0)
            {
                _repository.RemoveHome(home);
                await _repository.Commit();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetDLMState(SetDLMStateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _mqtt.PublishLockCommand(model.DLMId, model.Locked)) {
                    return Ok();
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPatch]
        public async Task<IActionResult> EditNickname(EditNicknameViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null) {
                return BadRequest();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            home.Nickname = model.Nickname;

            await _repository.Commit();

            return Ok();
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.DLMViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LiveBolt.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class DLMController : Controller
    {
        private readonly IRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DLMController(IRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            // TODO: Must check CompanyKey to ensure it was our module that sent the information
            // if (CompanyKey != OurKey) { return BadRequest() unauthorized? }

            if (ModelState.IsValid)
            {
                var storedDLM = await _repository.GetDLMByGuid(model.Id);
                if (storedDLM != null)
                {
                    return BadRequest($"Module with id: {model.Id} already created");
                }

                var dlm = new DLM
                {
                    Id = model.Id,
                    IsLocked = model.IsLocked
                };

                _repository.AddDLM(dlm);

                return Ok();
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null) {
                return BadRequest();
            }

            var dlm = await _repository.GetDLMByGuid(model.Guid);

            if (dlm == null) {
                return NotFound();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            dlm.Nickname = model.Nickname;
            dlm.AssociatedHomeId = home.Id;

            home.DLMs.Add(dlm);

            await _repository.Commit();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> EditNickname(EditNicknameViewModel model)
        {
            var dlm = await _repository.GetDLMByGuid(model.Guid);
            if (dlm == null)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null) {
                return BadRequest();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            if (!home.DLMs.Any(x => x.Id == model.Guid))
            {
                return BadRequest();
            }

            dlm.Nickname = model.Nickname;

            await _repository.Commit();

            return Ok();
        }
    }
}
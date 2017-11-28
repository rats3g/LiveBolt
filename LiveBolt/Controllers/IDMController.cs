using System.Linq;
using System.Threading.Tasks;
using LiveBolt.Data;
using LiveBolt.Models;
using LiveBolt.Models.IDMViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LiveBolt.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class IDMController : Controller
    {
        private readonly IRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public IDMController(IRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
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

                var idm = new IDM
                {
                    Id = model.Id,
                    IsClosed = model.IsClosed
                };

                _repository.AddIDM(idm);

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

            var idm = await _repository.GetIDMByGuid(model.Guid);

            if (idm == null) {
                return NotFound();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            idm.Nickname = model.Nickname;
            idm.AssociatedHomeId = home.Id;

            home.IDMs.Add(idm);

            await _repository.Commit();

            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> EditNickname(EditNicknameViewModel model)
        {
            var idm = await _repository.GetIDMByGuid(model.Guid);
            if (idm == null)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser.HomeId == null) {
                return BadRequest();
            }

            var home = await _repository.GetHomeById(currentUser.HomeId);

            if (!home.IDMs.Any(x => x.Id == model.Guid))
            {
                return BadRequest();
            }

            idm.Nickname = model.Nickname;

            await _repository.Commit();

            return Ok();
        }
    }
}
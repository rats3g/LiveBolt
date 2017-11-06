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

        [AllowAnonymous]
        public IActionResult Create(CreateViewModel model)
        {
            // TODO: Must check CompanyKey to ensure it was our module that sent the information
            // if (CompanyKey != OurKey) { return BadRequest() unauthorized? }

            if (ModelState.IsValid)
            {
                var dlm = new DLM
                {
                    Id = model.Id,
                    IsLocked = model.IsLocked
                };

                _repository.AddDLM(dlm);
            }

            return BadRequest(ModelState);
        }

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
            dlm.AssociatedHome = home;

            home.DLMs.Add(dlm);

            return Ok();
        }
    }
}
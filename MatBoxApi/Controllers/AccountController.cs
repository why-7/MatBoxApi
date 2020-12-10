using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatBoxApi.Models;
using MatBoxApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MatBoxApi.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        
        // GET
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if (_roleManager.Roles.Count(x => x.Name == "Admin") != 0 && 
                _roleManager.Roles.Count(x => x.Name == "Writer") != 0 &&
                _roleManager.Roles.Count(x => x.Name == "Reader") != 0)
                return View();
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _roleManager.CreateAsync(new IdentityRole("Writer"));
            await _roleManager.CreateAsync(new IdentityRole("Reader"));
            return View();
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        { 
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new User { Email = model.Email, UserName = model.Email};
            // добавляем пользователя
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // установка куки
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        
        [HttpGet("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
 
        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = 
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, 
                        model.RememberMe, false);
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Account");
                }
                ModelState.AddModelError("", "Неправильный логин и (или) пароль");
            }
            return View(model);
        }
        
        [HttpGet("LoginApi")]
        public async Task<IActionResult> LoginApi([FromHeader]string userName, [FromHeader]string password)
        {
            var result = await _signInManager.PasswordSignInAsync(userName, password, 
                true, false);
            return StatusCode(result.Succeeded ? 200 : 400);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Account");
        }
        
        [HttpGet("AccessDenied")]
        public StatusCodeResult AccessDenied(string returnUrl)
        {
            return StatusCode(403);
        }
        
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(string name)
        {
            string userId;
            
            if (_userManager.Users.Count(x => x.UserName == name) == 1)
            {
                 userId = _userManager.Users.First(x => x.UserName == name).Id;
            }
            else
            {            
                return NotFound();
            }
            
            // получаем пользователя
            var  user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            // получем список ролей пользователя
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            var model = new ChangeRoleViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserRoles = userRoles,
                AllRoles = allRoles
            };
            return View(model);

        }
        
        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            // получаем пользователя
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            // получем список ролей пользователя
            var userRoles = await _userManager.GetRolesAsync(user);
            // получаем все роли
            var allRoles = _roleManager.Roles.ToList();
            // получаем список ролей, которые были добавлены
            var addedRoles = roles.Except(userRoles);
            // получаем роли, которые были удалены
            var removedRoles = userRoles.Except(roles);
 
            await _userManager.AddToRolesAsync(user, addedRoles);
 
            await _userManager.RemoveFromRolesAsync(user, removedRoles);
 
            return RedirectToAction("Index", "Account");
        }
    }
}
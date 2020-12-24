using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Matbox.DAL.Models;
using Matbox.WEB.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Matbox.WEB.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("")]
        public IActionResult Index() => View(_roleManager.Roles.ToList());
 
        [Authorize(Roles = "Admin")]
        [HttpGet("CreateRole")]
        public IActionResult CreateRole() => View();
        
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(string name)
        {
            if (string.IsNullOrEmpty(name)) return View(name);
            var result = await _roleManager.CreateAsync(new IdentityRole(name));
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(name);
        }
         
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
            }
            return RedirectToAction("Index");
        }
 
        [Authorize(Roles = "Admin")]
        [HttpGet("UserList")]
        public IActionResult UserList() => View(_userManager.Users.ToList());
 
        [Authorize(Roles = "Admin")]
        [HttpGet("EditUserRoles")]
        public async Task<IActionResult> EditUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
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
        
        [Authorize(Roles = "Admin")]
        [HttpPost("EditUserRoles")]
        public async Task<IActionResult> EditUserRoles(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            var addedRoles = roles.Except(userRoles);
            var removedRoles = userRoles.Except(roles);
 
            await _userManager.AddToRolesAsync(user, addedRoles);
            await _userManager.RemoveFromRolesAsync(user, removedRoles);
 
            return RedirectToAction("UserList");

        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("CreateUser")]
        public IActionResult CreateUser()
        {
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
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
                return RedirectToAction("Index", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}
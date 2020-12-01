using MatBoxApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MatBoxApi.Controllers
{
    public class CreateUserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
 
        public CreateUserController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public void CreateUser()
        {
          //  var user = new User {UserName = "xyz", }
          //  _userManager.CreateAsync();
        }
    }
}
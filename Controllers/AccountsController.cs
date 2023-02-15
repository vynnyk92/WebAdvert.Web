using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _cognitoUserPool;

        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool cognitoUserPool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cognitoUserPool = cognitoUserPool;
        }

        [HttpGet]
        public async Task<IActionResult> Signup()
        {
            return View(new SignUpModel());
        }

        [HttpPost]
        public async Task<IActionResult> Signup([FromForm] SignUpModel signUpModel)
        {
            if (ModelState.IsValid)
            {
                var user = _cognitoUserPool.GetUser(signUpModel.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExist", "User already exist.");
                    return View(signUpModel);
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, signUpModel.Email);
                var createdUser = await _userManager.CreateAsync(user, signUpModel.Password);

                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm");
                }
            }

            return View(signUpModel);
        }

        [HttpGet]
        public async Task<IActionResult> Confirm()
        {
            return View(new ConfirmModel());
        }

        [HttpPost]
        public async Task<IActionResult> Confirm([FromForm] ConfirmModel confirmModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(confirmModel.Email);
                if (user is null)
                {
                    ModelState.AddModelError("NotFound", "A user given email address was not found");
                    return View(confirmModel);
                }

                if (_userManager is CognitoUserManager<CognitoUser> _cognitoUserManager)
                {
                    var result = await _cognitoUserManager.ConfirmSignUpAsync(user, confirmModel.Code, true);
                    return result.Succeeded ? RedirectToAction("Index", "Home") : View(confirmModel);
                }
            }


            return View(confirmModel);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password,
                    loginModel.RememberMe, false);

                return result.Succeeded ? RedirectToAction("Index", "Home") : View(loginModel);
            }


            return View(loginModel);
        }
    }
}

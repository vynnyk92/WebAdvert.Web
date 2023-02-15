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
        public async Task<IActionResult> Signup([FromBody]SignUpModel signUpModel)
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


            return View();
        }
    }
}

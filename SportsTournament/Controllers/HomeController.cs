using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportsTournament.Models;
using SportsTournament.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace VotingSystemProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ManagementSystemDbContext _context;

        public HomeController(ILogger<HomeController> logger, ManagementSystemDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // =====================================================
        // 🔐 AUTHENTICATION — LOGIN / REGISTER / LOGOUT
        // =====================================================

        public IActionResult Index() => View(new AuthViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var user = _context.Users
                .FirstOrDefault(u => u.Username == model.Login.Username && u.Password == model.Login.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View("Index", model);
            }

            if (user.Role == "PendingAdmin")
            {
                ModelState.AddModelError("", "Your admin request is pending approval.");
                return View("Index", model);
            }

            // ✅ Create authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }
            );

            // 🎯 Redirect by role
            return user.Role switch
            {
                "Admin" when user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)
                         || user.Username.Equals("mainadmin", StringComparison.OrdinalIgnoreCase)
                    => RedirectToAction("MainAdmin"),

                "Admin" => RedirectToAction("TournamentRecords"),
                _ => RedirectToAction("Router")
            };
        }

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
                Password = model.Register.Password, // ⚠️ Use hashing in production
                Role = finalRole
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (selectedRole == "Admin")
            {
                _context.MainAdmin.Add(new AdminRequest
                {
                    Username = newUser.Username,
                    Status = "Pending",
                    RequestedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();

                TempData["Message"] = "Admin request submitted. Awaiting main admin approval.";
                return RedirectToAction("AdminDetails");
            }

            TempData["Message"] = "Registration successful!";
            return RedirectToAction("UserDetails");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        public IActionResult AccessDenied() => View();

        // =====================================================
        // 🧾 ADMIN: MANAGE TOURNAMENT RECORDS
        // =====================================================

        [Authorize(Roles = "Admin")]
        public IActionResult TournamentRecords()
        {
            var allTournamentRecords = _context.TournamentRecords.ToList();
            var totalRecords = _context.TournamentRecords.Count();
            ViewBag.TotalRecords = totalRecords;
            return View(allTournamentRecords);


        }
        [Authorize(Roles = "User")]
        public IActionResult UserTable()
        {
            var username = User.Identity?.Name;

            // 🧠 If it's a user, show only their own records
            var records = User.IsInRole("Admin")
                ? _context.TournamentRecords.ToList()
                : _context.TournamentRecords.Where(r => r.Username == username).ToList();

            ViewBag.TotalRecords = records.Count;
            return View(records);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AcceptRecord(string id)
        {
            var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                record.Status = "Accepted";

                string message = $"Your booking (ID: {record.Id}) has been accepted ✅\n";

                // Estimated arrival times per route
                var estimatedTimes = new Dictionary<string, string>
        {
            {"Bien Unido-Trinidad", "11m-15"},
            {"Bien Unido-Talibon", "25m"},
            {"Bien Unido-Ubay", "35m"},
            {"Trinidad-Bien Unido", "1h 30m"},
            {"Trinidad-Ubay", "15m"},
            {"Trinidad-Talibon", "13m"},
            {"Talibon-Bien Unido", "25m"},
            {"Talibon-Ubay", "1h 10m"},
            {"Talibon-Trinidad", "13m"},
            {"Ubay-Bien Unido", "15m"},
            {"Ubay-Talibon", "1h 10m"},
            {"Ubay-Trinidad", "15m"}
        };

                // Prices per vehicle type and route
                var prices = new Dictionary<string, Dictionary<string, int>>
        {
            { "Bus", new Dictionary<string, int> {
                {"Bien Unido-Trinidad", 120}, {"Bien Unido-Talibon", 200}, {"Bien Unido-Ubay", 300},
                {"Trinidad-Bien Unido", 120}, {"Trinidad-Ubay", 250}, {"Trinidad-Talibon", 120},
                {"Talibon-Bien Unido", 200}, {"Talibon-Ubay", 180}, {"Talibon-Trinidad", 120},
                {"Ubay-Bien Unido", 300}, {"Ubay-Talibon", 180}, {"Ubay-Trinidad", 250}
            }},
            { "Van", new Dictionary<string, int> {
                {"Bien Unido-Trinidad", 150}, {"Bien Unido-Talibon", 250}, {"Bien Unido-Ubay", 350},
                {"Trinidad-Bien Unido", 150}, {"Trinidad-Ubay", 280}, {"Trinidad-Talibon", 150},
                {"Talibon-Bien Unido", 250}, {"Talibon-Ubay", 200}, {"Talibon-Trinidad", 150},
                {"Ubay-Bien Unido", 350}, {"Ubay-Talibon", 200}, {"Ubay-Trinidad", 280}
            }},
            { "Taxi", new Dictionary<string, int> {
                {"Bien Unido-Trinidad", 200}, {"Bien Unido-Talibon", 300}, {"Bien Unido-Ubay", 400},
                {"Trinidad-Bien Unido", 200}, {"Trinidad-Ubay", 350}, {"Trinidad-Talibon", 200},
                {"Talibon-Bien Unido", 300}, {"Talibon-Ubay", 250}, {"Talibon-Trinidad", 200},
                {"Ubay-Bien Unido", 400}, {"Ubay-Talibon", 250}, {"Ubay-Trinidad", 350}
            }}
        };

                string destination = record.Destination;
                string type = record.Type;
                int passengers = record.Passenger > 0 ? record.Passenger : 1; // fallback to 1 if missing

                string estimated = estimatedTimes.ContainsKey(destination) ? estimatedTimes[destination] : "N/A";
                int pricePerPerson = (prices.ContainsKey(type) && prices[type].ContainsKey(destination)) ? prices[type][destination] : 0;

                int totalPrice = pricePerPerson * passengers;

                // Make sure total price is shown in notification
                message += $"Route: {destination} | Estimated Arrival: {estimated} | Price per Passenger: ₱{pricePerPerson} | Total Price for {passengers} passenger(s): ₱{totalPrice}";

                // Add notification to inbox
                var notification = new Notification
                {
                    Recipient = record.Username,
                    Message = message,
                    DateCreated = DateTime.Now
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                TempData["Message"] = "Booking accepted and user notified!";
            }
            else
            {
                TempData["Error"] = "Record not found!";
            }

            return RedirectToAction("TournamentRecords");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult RejectRecord(string id)
        {
            var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                record.Status = "Rejected";

                // Create a notification for the booking user
                var notification = new Notification
                {
                    Recipient = record.Username,
                    Message = $"Your booking (ID: {record.Id}) has been rejected ❌",
                    DateCreated = DateTime.Now
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                TempData["Message"] = "Booking rejected and user notified!";
            }
            else
            {
                TempData["Error"] = "Record not found!";
            }

            return RedirectToAction("TournamentRecords");
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public IActionResult DeleteRecord(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid record ID.";
                return RedirectToAction("TournamentRecords");
            }

            var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("TournamentRecords");
            }

            _context.TournamentRecords.Remove(record);
            _context.SaveChanges();

            TempData["Message"] = "Record deleted successfully!";
            return RedirectToAction("TournamentRecords");
        }

        private IActionResult UpdateBookingStatus(string id, string newStatus, string successMessage)
        {
            var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);

            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("TournamentRecords");
            }

            record.Status = newStatus;
            _context.SaveChanges();

            TempData["Message"] = successMessage;
            return RedirectToAction("TournamentRecords");
        }

        // =====================================================
        // 🧍 USER: BOOKING FORM
        // =====================================================

        [Authorize(Roles = "User,Admin")]
        public IActionResult SportsTournament(string? id)
        {
            ViewBag.VehicleTypes = new SelectList(new[] { "Bus", "Van", "Taxi" });

            if (!string.IsNullOrEmpty(id))
            {
                var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);
                if (record != null)
                    return View(record);

                return NotFound();
            }

            string newId = GenerateBookingId();
            return View(new Records { Id = newId });
        }

        private string GenerateBookingId()
        {
            var last = _context.TournamentRecords
                .Where(r => r.Id.StartsWith("PSG"))
                .OrderByDescending(r => r.Id)
                .FirstOrDefault();

            if (last != null && int.TryParse(last.Id[3..], out int num))
                return $"PSG{num + 1}";

            return "PSG101";
        }

        [HttpPost, Authorize(Roles = "User,Admin"), ValidateAntiForgeryToken]
        public IActionResult Form(Records model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete all required fields.";
                return View("SportsTournament", model);
            }

            model.Username = User.Identity?.Name ?? "Unknown";
            var existing = _context.TournamentRecords.FirstOrDefault(r => r.Id == model.Id);

            if (existing == null)
            {
                model.Status = "Pending";
                _context.TournamentRecords.Add(model);
            }
            else
            {
                existing.FullName = model.FullName;
                existing.Type = model.Type;
                existing.Passenger = model.Passenger;
                existing.Location = model.Location;
                existing.Destination = model.Destination;
                existing.Status = model.Status;
                existing.PhoneNumber = model.PhoneNumber;
            }

            _context.SaveChanges();
            TempData["Message"] = "Booking submitted successfully!";
            return RedirectToAction("SportsTournament", new { id = model.Id });
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("SportsTournament");
            }

            TempData["Info"] = "You can now edit your booking details.";
            return View("SportsTournament", record);
        }

        [HttpPost]
        public IActionResult CancelBooking(string id)
        {
            var record = _context.TournamentRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("SportsTournament");
            }

            record.Status = "Cancelled";
            _context.SaveChanges();

            TempData["Message"] = "Your booking has been cancelled.";
            return RedirectToAction("SportsTournament", new { id });
        }

        // =====================================================
        // 👑 ADMIN MANAGEMENT (MAIN ADMIN)
        // =====================================================

        public IActionResult MainAdmin()
        {
            var requests = _context.MainAdmin.OrderByDescending(r => r.RequestedAt).ToList();
            return View(requests);
        }

        [HttpPost]
        public IActionResult AcceptAdmin(string username)
        {
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request == null || user == null)
            {
                TempData["Error"] = "Admin request not found.";
                return RedirectToAction("MainAdmin");
            }

            request.Status = "Accepted";
            user.Role = "Admin";
            _context.SaveChanges();

            TempData["Message"] = $"{username} has been promoted to Admin.";
            return RedirectToAction("MainAdmin");
        }

        [HttpPost]
        public IActionResult RejectAdmin(string username)
        {
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request == null || user == null)
            {
                TempData["Error"] = "Admin request not found.";
                return RedirectToAction("MainAdmin");
            }

            request.Status = "Rejected";
            user.Role = "User";
            _context.SaveChanges();

            TempData["Message"] = $"{username}'s admin request was rejected.";
            return RedirectToAction("MainAdmin");
        }

        [HttpPost]
        public IActionResult RemoveAdmin(string username)
        {
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request == null || user == null)
            {
                TempData["Error"] = "Admin not found.";
                return RedirectToAction("MainAdmin");
            }

            request.Status = "Removed";
            user.Role = "User";
            _context.SaveChanges();

            TempData["Message"] = $"{username} has been removed from Admin role.";
            return RedirectToAction("MainAdmin");
        }

        [HttpPost]
        public IActionResult DeleteDetails(string username)
        {
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            if (request != null)
            {
                _context.MainAdmin.Remove(request);
                _context.SaveChanges();
            }

            return RedirectToAction("MainAdmin");
        }

        // =====================================================
        // 👤 USER & ADMIN DETAILS
        // =====================================================

        [HttpGet] public IActionResult AdminDetails() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SubmitAdmin(AdminDetail model)
        {
            if (!ModelState.IsValid)
                return View("AdminDetails", model);

            _context.AdminDetails.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Admin details submitted successfully!";
            return RedirectToAction("AdminDetails");
        }

        [HttpGet] public IActionResult UserDetails() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SubmitUser(UserDetail model)
        {
            if (!ModelState.IsValid)
                return View("UserDetails", model);

            _context.UserDetails.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User details submitted successfully!";
            return RedirectToAction("UserDetails");
        }

        // =====================================================
        // ✉️ NOTIFICATIONS / INBOX
        // =====================================================

        [Authorize(Roles = "User,Admin")]
        public IActionResult Inbox()
        {
            var username = User.Identity?.Name;
            var notifications = _context.Notifications
                                        .Where(n => n.Recipient == username)
                                        .OrderByDescending(n => n.DateCreated)
                                        .ToList();
            return View(notifications);
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult DeleteNotification(int id)
        {
            var username = User.Identity?.Name;
            var notification = _context.Notifications
                .FirstOrDefault(n => n.Id == id && n.Recipient == username);

            if (notification == null)
            {
                TempData["Error"] = "Notification not found.";
                return RedirectToAction("Inbox");
            }

            _context.Notifications.Remove(notification);
            _context.SaveChanges();

            TempData["Message"] = "Notification deleted.";
            return RedirectToAction("Inbox");
        }

        // =====================================================
        // ⚙️ ROUTER / ERROR HANDLING
        // =====================================================

        public IActionResult Router() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

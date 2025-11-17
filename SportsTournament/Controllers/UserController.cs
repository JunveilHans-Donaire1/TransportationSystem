using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTournament.Models;
using SportsTournament.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VotingSystemProject.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ManagementSystemDbContext _context;
        public UserController(ManagementSystemDbContext context) => _context = context;

        // ===========================
        // Staff Details
        // ===========================
        [HttpGet]
        public IActionResult StaffDetails() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SubmitAdmin(StaffDetail model)
        {
            if (!ModelState.IsValid)
                return View("StaffDetails", model);

            _context.StaffDetails.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Staff details submitted successfully!";
            return RedirectToAction("StaffDetails");
        }

        // ===========================
        // User Table (My Bookings)
        // ===========================
        [Authorize(Roles = "User")]
        public IActionResult UserTable()
        {
            var username = User.Identity?.Name;
            var records = _context.TransportationRecords
                .Where(r => r.Username == username)
                .ToList();

            ViewBag.TotalRecords = records.Count;
            return View(records);
        }

        // ===========================
        // Edit Booking
        // ===========================
        public IActionResult EditBooking(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
                return NotFound();

            return View(record); // passes the record to the Edit view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBooking(Records model)
        {
            if (!ModelState.IsValid)
                return View(model); // show validation errors

            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == model.Id);
            if (record == null)
                return NotFound();

            // Update fields
            record.FullName = model.FullName;
            record.Type = model.Type;
            record.Passenger = model.Passenger;
            record.Location = model.Location;
            record.Destination = model.Destination;
            record.PhoneNumber = model.PhoneNumber;

            _context.SaveChanges(); // persist changes

            TempData["Message"] = "Booking updated successfully!";
            return RedirectToAction("UserTable"); // redirect to bookings table
        }


        // ===========================
        // Cancel Booking
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelBooking(string id)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("UserTable");
            }

            _context.TransportationRecords.Remove(record);
            _context.SaveChanges();

            TempData["Message"] = "Booking cancelled successfully!";
            return RedirectToAction("UserTable");
        }

        // ===========================
        // Feedback
        // ===========================
        public IActionResult Feedbacks() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.CreatedAt = System.DateTime.UtcNow;

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Feedback submitted successfully!";
                return RedirectToAction("Feedbacks");
            }

            return View(feedback);
        }

        // ===========================
        // User Profile
        // ===========================
        [Authorize]
        public IActionResult UsersProfile()
        {
            var gmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(gmail))
            {
                TempData["Error"] = "Cannot find your email. Please log in again.";
                return RedirectToAction("Index", "Auth");
            }

            var userDetails = User.IsInRole("Admin")
                ? _context.UserDetails.ToList()
                : _context.UserDetails.Where(u => u.Gmail == gmail).ToList();

            return View(userDetails);
        }




        // ===========================
        // Submit User Details
        // ===========================
        public IActionResult UserDetails() => View();

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
    }
}

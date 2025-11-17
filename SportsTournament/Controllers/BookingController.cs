using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportsTournament.Models;
using System.Linq;

namespace VotingSystemProject.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class BookingController : Controller
    {
        private readonly ManagementSystemDbContext _context;
        public BookingController(ManagementSystemDbContext context) => _context = context;

        // ===========================
        // Display booking form
        // ===========================
        public IActionResult Transportation(string? id)
        {
            ViewBag.VehicleTypes = new SelectList(new[] { "Bus", "Van", "Taxi" });

            if (!string.IsNullOrEmpty(id))
            {
                var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
                if (record != null)
                    return View(record);

                return NotFound();
            }

            string newId = GenerateBookingId();
            return View(new Records { Id = newId });
        }

        private string GenerateBookingId()
        {
            var last = _context.TransportationRecords
                .Where(r => r.Id.StartsWith("JJGGR"))
                .OrderByDescending(r => r.Id)
                .FirstOrDefault();

            if (last != null && int.TryParse(last.Id[5..], out int num)) // [5..] because JJGGR = 5 chars
                return $"JJGGR{num + 1}";

            return "JJGGR101";
        }

        // ===========================
        // Create or edit booking
        // ===========================
        [HttpPost, Authorize(Roles = "User,Admin"), ValidateAntiForgeryToken]
        public IActionResult Form(Records model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete all required fields.";
                return View("Transportation", model);
            }

            model.Username = User.Identity?.Name ?? "Unknown";
            var existing = _context.TransportationRecords.FirstOrDefault(r => r.Id == model.Id);

            if (existing == null)
            {
                model.Status = "Pending";
                _context.TransportationRecords.Add(model);
            }
            else
            {
                // Update existing record
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

            // Redirect to UserTable in UserController
            return RedirectToAction("UserTable", "User");
        }
    }
}

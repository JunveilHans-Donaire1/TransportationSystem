using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTournament.Models;
using System.Collections.Generic;
using System.Linq;

namespace VotingSystemProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ManagementSystemDbContext _context;
        public AdminController(ManagementSystemDbContext context) => _context = context;
      
        public IActionResult TransportationRecords()
        {
            var allTournamentRecords = _context.TransportationRecords.ToList();
            var totalRecords = _context.TransportationRecords.Count();
            ViewBag.TotalRecords = totalRecords;
            return View(allTournamentRecords);
        }
     




        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AcceptRecord(string id)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                record.Status = "Accepted";
                string message = $"Your booking (ID: {record.Id}) has been accepted ✅\n";

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
                int passengers = record.Passenger > 0 ? record.Passenger : 1;

                string estimated = estimatedTimes.ContainsKey(destination) ? estimatedTimes[destination] : "N/A";
                int pricePerPerson = (prices.ContainsKey(type) && prices[type].ContainsKey(destination)) ? prices[type][destination] : 0;
                int totalPrice = pricePerPerson * passengers;

                message += $"Route: {destination} | Estimated Arrival: {estimated} | Price per Passenger: ₱{pricePerPerson} | Total Price for {passengers} passenger(s): ₱{totalPrice}";

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

            return RedirectToAction("TransportationRecords");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult RejectRecord(string id)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                record.Status = "Rejected";
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

            return RedirectToAction("TransportationRecords");
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public IActionResult DeleteRecord(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid record ID.";
                return RedirectToAction("TransportationRecords");
            }

            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("TransportationRecords");
            }

            _context.TransportationRecords.Remove(record);
            _context.SaveChanges();

            TempData["Message"] = "Record deleted successfully!";
            return RedirectToAction("TransportationRecords");
        }

        private IActionResult UpdateBookingStatus(string id, string newStatus, string successMessage)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);

            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("TransportationRecords");
            }

            record.Status = newStatus;
            _context.SaveChanges();

            TempData["Message"] = successMessage;
            return RedirectToAction("TournamentRecords");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SportsTournament.Models;
using SportsTournament.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VotingSystemProject.Controllers
{
    public class MainAdminController : Controller
    {
        private readonly ManagementSystemDbContext _context;
        public MainAdminController(ManagementSystemDbContext context) => _context = context;

        public IActionResult MainAdmin()
        {
            // Get all Admin Requests
            var adminRequests = _context.MainAdmin.ToList();

            // Get all Booking Records
            var bookingRecords = _context.TransportationRecords.ToList();

            // Get all User Details
            var userDetails = _context.UserDetails.ToList(); // replace with your actual DbSet

            // Get all Admin Details
            var adminDetails = _context.StaffDetails.ToList(); // replace with your actual DbSet

            var users = _context.Users.ToList();
            var feedbacks = _context.Feedbacks.ToList();

            // Pass all four datasets as a Tuple
            var model = Tuple.Create<IEnumerable<StaffRequest>, IEnumerable<Records>, IEnumerable<UserDetail>, IEnumerable<StaffDetail>, IEnumerable<User>, IEnumerable<Feedback>>(
                adminRequests,
                bookingRecords,
                userDetails,
                adminDetails,
                users,
                feedbacks
            );

            return View(model);
        }



        [HttpPost]
        public IActionResult AcceptAdmin(string username)
        {
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request == null || user == null)
            {
                TempData["Error"] = "Staff request not found.";
                return RedirectToAction("MainAdmin");
            }

            request.Status = "Accepted";
            user.Role = "Admin";
            _context.SaveChanges();

            TempData["Message"] = $"{username} You are now registered as a staff member!.";
            return RedirectToAction("MainAdmin");
        }

        [HttpPost]
        public IActionResult RejectAdmin(string username)
        {
            // Find the admin request
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            // Find the associated user
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request == null || user == null)
            {
                TempData["Error"] = "Staff request or user not found.";
                return RedirectToAction("MainAdmin");
            }

            // Remove both the admin request and the user account
            _context.MainAdmin.Remove(request);
            _context.Users.Remove(user);

            _context.SaveChanges();

            TempData["Message"] = $"{username}'s Staff request was rejected and the account has been deleted.";
            return RedirectToAction("MainAdmin");
        }



        [HttpPost]
        public IActionResult RemoveAdmin(string username)
        {
            var request = _context.MainAdmin.FirstOrDefault(r => r.Username == username);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (request == null || user == null)
            {
                TempData["Error"] = "Staff not found.";
                return RedirectToAction("MainAdmin");
            }

            request.Status = "Removed";
            user.Role = "User";
            _context.SaveChanges();

            TempData["Message"] = $"{username} has been removed from Staff role.";
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
        [HttpPost]
        public IActionResult AcceptRecord(string id)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("MainAdmin");
            }

            record.Status = "Accepted";

            // Optionally send notification to user
            _context.Notifications.Add(new Notification
            {
                Recipient = record.Username,
                Message = $"Your booking (ID: {record.Id}) has been accepted ✅",
                DateCreated = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Message"] = "Booking accepted and user notified!";
            return RedirectToAction("MainAdmin");
        }

        [HttpPost]
        public IActionResult RejectRecord(string id)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("MainAdmin");
            }

            record.Status = "Rejected";

            // Notify user
            _context.Notifications.Add(new Notification
            {
                Recipient = record.Username,
                Message = $"Your booking (ID: {record.Id}) has been rejected ❌",
                DateCreated = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Message"] = "Booking rejected and user notified!";
            return RedirectToAction("MainAdmin");
        }

        [HttpPost]
        public IActionResult DeleteRecord(string id)
        {
            var record = _context.TransportationRecords.FirstOrDefault(r => r.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found!";
                return RedirectToAction("MainAdmin");
            }

            _context.TransportationRecords.Remove(record);
            _context.SaveChanges();

            TempData["Message"] = "Record deleted successfully!";
            return RedirectToAction("MainAdmin");
        }
    }
}

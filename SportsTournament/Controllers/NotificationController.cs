using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTournament.Models;
using System.Linq;

namespace VotingSystemProject.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class NotificationController : Controller
    {
        private readonly ManagementSystemDbContext _context;
        public NotificationController(ManagementSystemDbContext context) => _context = context;

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
    }
}

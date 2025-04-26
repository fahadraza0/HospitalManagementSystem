using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View("AdminDashboard");
        }
        public IActionResult DoctorDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }
        public IActionResult StaffDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }
    }
}

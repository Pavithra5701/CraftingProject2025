using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace CraftingProject.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                // Check where to redirect based on the logged-in user
                var user = HttpContext.Session.GetString("Username");
                if (user == "user")
                    return RedirectToAction("Dashboard");
                else if (user == "admin")
                    return RedirectToAction("AdminDashboard");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            if (username == "user" && password == "password") 
            {
                HttpContext.Session.SetString("Username", username);
                return RedirectToAction("Dashboard", "Home");
            }
            else if (username == "admin" && password == "password") 
            {
                HttpContext.Session.SetString("Username", username);
                return RedirectToAction("AdminDashboard");
            }

            ViewBag.Message = "Invalid username or password";
            return View();
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Username") == "user")
            {
                return View();
            }
            return RedirectToAction("Index");
        }

        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("Username") == "admin")
            {
                return RedirectToAction("AdminDashboard", "Admin"); // Full view path
            }
            return RedirectToAction("Index", "Login");
        }

       



        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}

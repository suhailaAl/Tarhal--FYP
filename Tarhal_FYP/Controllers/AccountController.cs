using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tarhal_FYP.Models.ModelViews;
using Tarhal_FYP.Models;

namespace Tarhal_FYP.Controllers
{
    public class AccountController : Controller
    {
        TarhalDbContext db = new TarhalDbContext();

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exists in the database
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    // Create claims for user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.UserType)
                    };

                    // Create identity and principal
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe // Remember me option
                    };

                    // Sign in the user
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username already exists
                if (db.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Username already exists.");
                }
                else if (db.Tourists.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already exists.");
                }
                else
                {
                    // Create a new user and add to the database
                    var newUser = new User
                    {
                        Username = model.Username,
                        Password = model.Password, 
                        UserType = "Tourist" 
                    };
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    var newTourist = new Tourist
                    {
                        TouristId = newUser.UserId,
                        Name = model.Name,
                        Email = model.Email,
                        TravelTime = 10
                    };
                    db.Tourists.Add(newTourist);
                    db.SaveChanges();

                    return RedirectToAction("Login");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Upload(List<IFormFile> ImageFiles)
        //{
        //    int id = db.Activities.First().ActivityId;
        //    foreach (IFormFile image in ImageFiles)
        //    {
        //        if (image != null && image.Length > 0)
        //        {
        //            using (var memoryStream = new MemoryStream())
        //            {
        //                image.CopyTo(memoryStream);

        //                // Update the ImageData field in the Accommodation table
        //                var service = db.Activities.FirstOrDefault(a => a.ActivityId == id);
        //                if (service != null)
        //                {
        //                    db.Images.Add(new Image
        //                    {
        //                        ServiceId = id,
        //                        ImageData = memoryStream.ToArray()
        //                    });
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    return NotFound("Service not found");
        //                }
        //            }
        //        }
        //        id++;
        //    }

        //    return RedirectToAction("Index");
        //}
    }
}

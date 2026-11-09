using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using Tarhal_FYP.Models;

namespace Tarhal_FYP.Controllers
{
    public class ServiceProviderController : Controller
    {
        TarhalDbContext db = new TarhalDbContext();
        public IActionResult ProviderDashboard()
        {
            var provider = db.ServiceProviders.First();
            ViewBag.Provider = provider;
            var services = db.Activities.Where(s => s.ProviderId == provider.ProviderId);
            ViewBag.services = services;
            return View();
        }
        public IActionResult New_Service_Form()
        {
            return View();
        }

        [HttpPost]
        public IActionResult serviceForm(string serviceKind)
        {
            return View();
        }
        public IActionResult Update_Service(int id)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceId == id);
            
            return View(service);
        }
    }
}

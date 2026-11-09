using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tarhal_FYP.Models;

namespace Tarhal_FYP.Controllers
{
    public class TouristController : Controller
    {
        TarhalDbContext db = new TarhalDbContext();
        private readonly EmailSender _emailSender;

        public TouristController(EmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult Plan_Form()
        {
            ViewBag.hideForm = false;
            return View();
        }

        [HttpPost]
        public IActionResult planForm1(string planName, string DestinedGodernorate, string tripKind, int price, List<string> preferences, string daterange)
        {
            int tourist_id = 3;
            var budget = new string('$', price);
            var dates = daterange.Split(" - ");

            DateOnly startDate = DateOnly.Parse(dates[0]);
            DateOnly endDate = DateOnly.Parse(dates[1]);

            string prefrence = "";
            foreach (string p in preferences)
            {
                prefrence += $"{p},";
            }

            if (prefrence.EndsWith(",")) { prefrence = prefrence.TrimEnd(','); }

            Plan plan = new Plan
            {
                Name = planName,
                Destination = DestinedGodernorate,
                TouristId = tourist_id,
                DateBegin = startDate,
                DateEnd = endDate,
                Budget = budget,
                Preferences = prefrence,
                TripType = tripKind
            };

            //create new plan in database
            try
            {
                db.Plans.Add(plan);

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // generate recommendations
            Recommendation_System rs = new Recommendation_System();

            List<int> recommended_accommodations = rs.recommend_Accommodations(plan);
            List<int> recommended_restaurants = rs.recommend_Restaurants(plan);
            List<int> recommended_attractions = rs.recommend_Attractions(plan);
            List<int> recommended_activities = rs.recommend_Activities(plan);

            // send to view using viewBag
            ViewBag.ac_ids = recommended_accommodations;
            ViewBag.res_ids = recommended_restaurants;
            ViewBag.at_ids = recommended_attractions;
            ViewBag.act_ids = recommended_activities;

            ViewBag.hideForm = true;
            TempData["planId"] = plan.PlanId;

            return View("Plan_Form");
        }

        [HttpPost]
        public IActionResult planForm2(List<int> Accommodations, List<int> Restaurants, List<int> Attractions, List<int> Activities)
        {
            // get plan id from TempData
            int planId = (int)TempData["planId"];

            //--------------------------------------------------------------------------------------------------------------------------
            // generate day-by-day plan

            DateOnly startDate = db.Plans.Find(planId).DateBegin;
            DateOnly endDate = db.Plans.Find(planId).DateEnd;

            Plan_Generator pg = new Plan_Generator();
            var plan = pg.generate(endDate.DayNumber - startDate.DayNumber + 1, Accommodations, Restaurants, Attractions, Activities);
            ViewBag.plan = plan;

            //--------------------------------------------------------------------------------------------------------------------------
            // prepare routes and markers for map
            var locations = new List<dynamic>();
            var planRoutes = new List<List<dynamic>>();
            foreach (var day in plan)
            {
                foreach (var stop in day)
                {
                    var dayStops = new List<dynamic>();
                    switch (stop.Type)
                    {
                        case "Accommodation":
                            dayStops.Add(new
                            {
                                name = stop.Name,
                                lat = stop.Latitude,
                                lng = stop.Longitude,
                                address = db.Accommodations.Find(stop.Id).Address,
                                price = db.Accommodations.Find(stop.Id).PricePerNight,
                                category = "hotel"
                            });
                            break;
                        case "Restaurant":
                            dayStops.Add(new
                            {
                                name = stop.Name,
                                lat = stop.Latitude,
                                lng = stop.Longitude,
                                address = db.Restaurants.Find(stop.Id).Address,
                                price = db.Restaurants.Find(stop.Id).PriceRange,
                                category = "utensils"
                            });
                            break;
                        case "Attraction":
                            dayStops.Add(new
                            {
                                name = stop.Name,
                                lat = stop.Latitude,
                                lng = stop.Longitude,
                                address = db.Attractions.Find(stop.Id).Address,
                                price = db.Attractions.Find(stop.Id).AdmissionFee,
                                category = "camera"
                            });
                            break;
                        case "Activity":
                            dayStops.Add(new
                            {
                                name = stop.Name,
                                lat = stop.Latitude,
                                lng = stop.Longitude,
                                address = db.Activities.Find(stop.Id).Address,
                                price = db.Activities.Find(stop.Id).Price,
                                category = "person-walking"
                            });
                            break;
                        default:
                            break;
                    }
                    planRoutes.Add(dayStops);
                }
            }
            //foreach (int i in Accommodations)
            //{
            //    var s = db.Accommodations.Find(i);
            //    locations.Add(new{
            //        name = s.Name,
            //        lat = s.Latitude,
            //        lng = s.Longitude,
            //        address = s.Address,
            //        price = s.PricePerNight,
            //        category = "hotel"
            //    });
            //}

            //foreach (int i in Restaurants)
            //{
            //    var s = db.Restaurants.Find(i);
            //    locations.Add(new
            //    {
            //        name = s.Name,
            //        lat = s.Latitude,
            //        lng = s.Longitude,
            //        address = s.Address,
            //        price = s.PriceRange,
            //        category = "utensils"
            //    });
            //}

            //foreach (int i in Attractions)
            //{
            //    var s = db.Attractions.Find(i);
            //    locations.Add(new
            //    {
            //        name = s.Name,
            //        lat = s.Latitude,
            //        lng = s.Longitude,
            //        address = s.Address,
            //        price = s.AdmissionFee,
            //        category = "camera"
            //    });
            //}

            //foreach (int i in Activities)
            //{
            //    var s = db.Activities.Find(i);
            //    locations.Add(new
            //    {
            //        name = s.Name,
            //        lat = s.Latitude,
            //        lng = s.Longitude,
            //        address = s.Address,
            //        price = s.Price,
            //        category = "person-walking"
            //    });
            //}

            //--------------------------------------------------------------------------------------------------------------------------
            // send data to plan page using ViewBag
            ViewBag.name = db.Plans.Find(planId).Name;
            ViewBag.des = db.Plans.Find(planId).Destination;
            ViewBag.dur = endDate.DayNumber - startDate.DayNumber + 1;
            ViewBag.StartDate = startDate;
            ViewBag.accommodation = db.Accommodations
                .Where(e => Accommodations.Contains(e.AccommodationId))
                .Select(e => e.Name)
                .ToList();
            ViewBag.restaurant = db.Restaurants
                .Where(e => Restaurants.Contains(e.RestaurantId))
                .Select(e => e.Name)
                .ToList();
            ViewBag.attraction = db.Attractions
                .Where(e => Attractions.Contains(e.AttractionId))
                .Select(e => e.Name)
                .ToList();
            ViewBag.activity = db.Activities
                .Where(e => Activities.Contains(e.ActivityId))
                .Select(e => e.Name)
                .ToList();
            ViewBag.locations = locations;

            return View("Plan");
        }

        // display recommendation details using AJAX
        public JsonResult GetRecommendationDetails(int id, string table)
        {
            var result = new { name = "", description = "", providerName = "" };

            switch (table)
            {
                case "Accommodation":
                    result = (from a in db.Accommodations
                              join p in db.ServiceProviders on a.ProviderId equals p.ProviderId
                              where a.AccommodationId == id
                              select new
                              {
                                  name = a.Name,
                                  description = a.Description,
                                  providerName = p.Name,
                              })
                              .FirstOrDefault() ?? result;
                    break;

                case "Restaurants":
                    result = (from a in db.Restaurants
                              join p in db.ServiceProviders on a.ProviderId equals p.ProviderId
                              where a.RestaurantId == id
                              select new
                              {
                                  name = a.Name,
                                  description = a.Description,
                                  providerName = p.Name,
                              })
                              .FirstOrDefault() ?? result;
                    break;

                case "Attractions":
                    result = (from a in db.Attractions
                              join p in db.ServiceProviders on a.ProviderId equals p.ProviderId
                              where a.AttractionId == id
                              select new
                              {
                                  name = a.Name,
                                  description = a.Description,
                                  providerName = p.Name,
                              })
                              .FirstOrDefault() ?? result;
                    break;

                case "Activities":
                    result = (from a in db.Activities
                              join p in db.ServiceProviders on a.ProviderId equals p.ProviderId
                              where a.ActivityId == id
                              select new
                              {
                                  name = a.Name,
                                  description = a.Description,
                                  providerName = p.Name,
                              })
                              .FirstOrDefault() ?? result;
                    break;

                default:
                    return Json(new { details = "Invalid table specified." });
            }

            // Handle cases where the record is not found
            if (result == null)
            {
                return Json(new { details = "No details found for the given ID." });
            }

            return Json(result);
        }

        public IActionResult Plan()
        {

            return View();
        }

        public IActionResult TouristDashboard()
        {
            return View();
        }

        public async Task<IActionResult> Rating_Form()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit_Rating(IFormCollection form)
        {
            int PlanId = 1;
            var plan = await db.Plans.FindAsync(PlanId);

            // Plan days
            var planDays = new List<List<dynamic>>();
            for (int i = 0; i < (plan.DateEnd.DayNumber - plan.DateBegin.DayNumber + 1); i++) { planDays.Add(new List<dynamic>()); }

            // Retrieve Services and assign to plan days
            List<dynamic> services =
            [
                .. await db.PlanActivities.Where(a => a.PlanId == PlanId).ToListAsync(),
                .. await db.PlanAttractions.Where(a => a.PlanId == PlanId).ToListAsync(),
                .. await db.PlanRestaurants.Where(a => a.PlanId == PlanId).ToListAsync(),
                .. await db.PlanAccommodations.Where(a => a.PlanId == PlanId).ToListAsync(),
            ];
            foreach (var service in services) { planDays[service.DayNum - 1].Add(service); }

            var ratings = new List<int>();
            for (int i = 0; i < planDays.Count; i++)
            {
                ratings.Add(int.Parse(form[$"rating_{i}"]));
            }

            ViewBag.Ratings = ratings;

            // Update average rating
            //foreach (var day in planDays)
            //{
            //    int i = 0;
            //    foreach (var service in day)
            //    {
            //        service.TotalReviews++;
            //        service.Rating = ((service.Rating * (service.TotalReviews - 1)) + ratings[i]) / service.TotalReviews;

            //        db.Update(service);
            //        await db.SaveChangesAsync();

            //        i++;
            //    }
            //}

            return RedirectToAction("Rating_Form");
        }

        public async Task CompleteTrip(int planId)
        {
            var now = DateOnly.FromDateTime(DateTime.Now);
            var endedTrips = await db.Plans
                .Where(t => t.DateEnd <= now).ToListAsync();

            foreach (var trip in endedTrips)
            {
                var tourist = await db.Tourists.FindAsync(trip.TouristId);
                var ratingUrl = Url.Action("RatePlace", "Place", new { tripId = trip.PlanId }, Request.Scheme);

                var emailBody = $@"
                <h2>How was your trip?</h2>
                <p>We'd love your feedback on the places you visited.</p>
                <a href='{ratingUrl}'>Rate Your Experience</a>
            ";

                await _emailSender.SendEmailAsync(tourist.Email, "Rate Your Experience", emailBody);

                db.Update(trip);
                await db.SaveChangesAsync();
            }
        }


        public IActionResult Nearby()
        {
            return View();
        }

        private readonly string apiKey = "AIzaSyCJifUpVnSdInDEmbOgyyZpOAWwYZJMp0E";

        [HttpGet]
        public async Task<IActionResult> GetNearby(double latitude, double longitude)
        {
            var types = new List<string> { "tourist_attraction", "restaurant", "hotel", "park" };
            var allResults = new List<object>();

            using var client = new HttpClient();

            foreach (var type in types)
            {
                string url = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={latitude},{longitude}&radius=50000&type={type}&key={apiKey}";
                var response = await client.GetStringAsync(url);

                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("results", out JsonElement results))
                {
                    foreach (var place in results.EnumerateArray())
                    {
                        allResults.Add(new
                        {
                            name = place.GetProperty("name").GetString(),
                            lat = place.GetProperty("geometry").GetProperty("location").GetProperty("lat").GetDouble(),
                            lng = place.GetProperty("geometry").GetProperty("location").GetProperty("lng").GetDouble(),
                            type = type, // Include type for frontend coloring
                            rating = place.TryGetProperty("rating", out var rating) ? rating.GetDouble() : 0,
                            vicinity = place.TryGetProperty("vicinity", out var vicinity) ? vicinity.GetString() : "N/A"
                        });
                    }
                }
            }

            return Json(allResults);
        }
    }
}

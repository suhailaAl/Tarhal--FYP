using Microsoft.Identity.Client;
using System;
using System.Linq;
namespace Tarhal_FYP.Models
{
    public class Plan_Generator
    {
        TarhalDbContext db = new TarhalDbContext();
        public List<List<ServiceClusterData>> generate(int duration, List<int> ac_ids,List<int> res_ids,List<int> at_ids,List<int> act_ids ) {

            List<List<ServiceClusterData>> plan = new List<List<ServiceClusterData>>();
            List<ServiceClusterData> services = new List<ServiceClusterData>();
            List<ServiceClusterData> accommodations = new List<ServiceClusterData>();
            List<ServiceClusterData> restaurants = new List<ServiceClusterData>();

            foreach (int i in ac_ids)
            {
                var s = db.Accommodations.Find(i);
                accommodations.Add(new ServiceClusterData
                {
                    Id = s.AccommodationId,
                    Name = s.Name,
                    Latitude = (float) s.Latitude,
                    Longitude = (float) s.Longitude,
                    Duration = 0,
                    Type = "Accommodation"
                });
            }

            foreach (int i in res_ids)
            {
                var s = db.Restaurants.Find(i);
                restaurants.Add(new ServiceClusterData
                {
                    Id = s.RestaurantId,
                    Name = s.Name,
                    Latitude = (float)s.Latitude,
                    Longitude = (float)s.Longitude,
                    Duration = 1,
                    Type = s.CuisineType
                });
            }

            foreach (int i in at_ids)
            {
                var s = db.Attractions.Find(i);
                services.Add(new ServiceClusterData
                {
                    Id = s.AttractionId,
                    Name = s.Name,
                    Latitude = (float) s.Latitude,
                    Longitude = (float) s.Longitude,
                    Duration = (double)s.TimeTaken,
                    Type = "Attraction"
                });
            }

            foreach (int i in act_ids)
            {
                var s = db.Activities.Find(i);
                services.Add(new ServiceClusterData
                {
                    Id = s.ActivityId,
                    Name = s.Name,
                    Latitude = (float)s.Latitude,
                    Longitude = (float)s.Longitude,
                    Duration = (double)s.TimeTaken,
                    Type = "Activity"
                });
            }
            
            // cluster using accommodations as centroids
            KMeansClustering kMeansClustring = new KMeansClustering(accommodations.Count, accommodations);
            
            var clusters = kMeansClustring.Cluster(services);

            var orderedClusters = clusters
                        .ToDictionary(
                        cluster => cluster.Key,
                        cluster => cluster.Value.OrderBy(service => CalculateDistance(cluster.Key.Latitude, cluster.Key.Longitude, service.Latitude, service.Longitude))
                        .ToList()
                        )
                        .OrderByDescending(cluster => cluster.Value.Sum(service => service.Duration))
                        .ToDictionary(cluster => cluster.Key, cluster => cluster.Value);

            // track remaining days
            int remainDays = duration;

            Console.WriteLine($"Clusters = {clusters.Count}");
            // 
            for (int c = 0; c < orderedClusters.Count; c++)
            {
                if (orderedClusters.ElementAt(c).Value.Count == 0)
                {
                    continue;
                }
                
                int totalDays = 0;
                if (c == orderedClusters.Count - 1)
                {
                    totalDays = remainDays;
                }
                else
                {
                    totalDays = (int)(orderedClusters.ElementAt(c).Value.Sum(service => service.Duration) / 8);
                }

                if (totalDays <= remainDays && totalDays != 0)
                {
                    KMeansClustering dayClustring = new KMeansClustering(totalDays);

                    var days = dayClustring.Cluster(orderedClusters.ElementAt(c).Value);

                    // order clusters by total duration
                    days = days
                        .ToDictionary(
                        day => day.Key,
                        day => day.Value.OrderBy(service => CalculateDistance(day.Key.Latitude, day.Key.Longitude, service.Latitude, service.Longitude))
                        .ToList()
                        )
                        .OrderByDescending(day => day.Value.Sum(service => service.Duration))
                        .ToDictionary(day => day.Key, day => day.Value);
                    

                    for (int i = 0; i<days.Count; i++)
                    {

                        int totalDuration = (int) days.ElementAt(i).Value.Sum(service => service.Duration);

                        while (totalDuration > 8)
                        {
                            if (i != days.Count - 1)
                            {
                                // move farthest to next day
                                var last = days.ElementAt(i).Value.Last();
                                days.ElementAt(i).Value.RemoveAt(days.ElementAt(i).Value.Count - 1);
                                days.ElementAt(i + 1).Value.Add(last);
                            }
                            else
                            {
                                if (c == clusters.Count - 1) { break; }
                                // move to next cluster
                                var last = days.ElementAt(i).Value.Last();
                                days.ElementAt(i).Value.RemoveAt(days.ElementAt(i).Value.Count - 1);
                                orderedClusters.ElementAt(c + 1).Value.Add(last);
                            }

                            // update total duration
                            totalDuration = (int) days.ElementAt(i).Value.Sum(service => service.Duration);
                        }

                        // add day accomodation
                        days.ElementAt(i).Value.Insert(0, accommodations[clusters.Keys.ToList().IndexOf(orderedClusters.ElementAt(c).Key)]);

                        // add planned day to plan list
                        plan.Add(days.ElementAt(i).Value);

                    }
                    // update remainning unplanned days
                    remainDays -= days.Count;
                }
                else
                {
                    foreach ( var s in orderedClusters.ElementAt(c).Value)
                    {
                        var leastDurationDay = plan.OrderBy(day => day.Sum(s => s.Duration)).First();
                        leastDurationDay.Add(s);
                    }
                }
            }
            // include resturants 
            foreach (var day in plan)
            {
                // first restaurant
                ServiceClusterData closestResturant = restaurants.OrderBy(r => CalculateDistance(r.Latitude, r.Longitude, day[1].Latitude, day[1].Longitude)).First();
                if (closestResturant.Type.Contains("Breakfast"))
                {
                    day.Insert(1, closestResturant);
                }
                else
                {
                    day.Insert(2, closestResturant);
                }
                
                // second restaurant
                ServiceClusterData closestResturant2 = restaurants.OrderBy(r => CalculateDistance(r.Latitude, r.Longitude, day[^1].Latitude, day[^1].Longitude)).First();
                 
                int i = 0;
                while(closestResturant == closestResturant2)
                {
                    closestResturant2 = restaurants.OrderBy(r => CalculateDistance(r.Latitude, r.Longitude, day[0].Latitude, day[0].Longitude)).ElementAt(i);
                    i++;
                }
                day.Add(closestResturant2);
            }

            return plan;
        }

        static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distance in km
        }
    }
}

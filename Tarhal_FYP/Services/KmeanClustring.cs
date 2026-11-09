namespace Tarhal_FYP.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class KMeansClustering
    {
        public List<ServiceClusterData> Centroids { get; private set; }
        public int K { get; private set; }

        public KMeansClustering(int k, List<ServiceClusterData> initialCentroids = null)
        {
            K = k;
            Centroids = initialCentroids ?? new List<ServiceClusterData>();
        }

        public Dictionary<ServiceClusterData, List<ServiceClusterData>> Cluster(List<ServiceClusterData> points, int maxIterations = 100)
        {
            
            // Randomly initialize K centroids from points in case no centroids provided
            if (Centroids.Count == 0)
            {
                Random random = new Random();
                Centroids = points.OrderBy(x => random.Next()).Take(K).ToList();
            }

            var clusters = new Dictionary<ServiceClusterData, List<ServiceClusterData>>();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Assign points to the nearest centroid
                clusters = AssignPointsToClusters(points);

                // Update centroids based on the mean of assigned points
                var newCentroids = UpdateCentroids(clusters);

                // Check for convergence (if centroids don't change, we stop)
                if (AreCentroidsConverged(newCentroids))
                {
                    Console.WriteLine($"Converged after {iteration + 1} iterations.");
                    break;
                }

                // Update centroids for the next iteration
                Centroids = newCentroids;
            }

            return clusters;
        }

        private Dictionary<ServiceClusterData, List<ServiceClusterData>> AssignPointsToClusters(List<ServiceClusterData> points)
        {
            var clusters = Centroids.ToDictionary(c => c, c => new List<ServiceClusterData>());

            foreach (var point in points)
            {
                ServiceClusterData closestCentroid = Centroids.OrderBy(c => CalculateDistance(c.Latitude, c.Longitude, point.Latitude, point.Longitude)).First();
                clusters[closestCentroid].Add(point);
            }

            return clusters;
        }

        private List<ServiceClusterData> UpdateCentroids(Dictionary<ServiceClusterData, List<ServiceClusterData>> clusters)
        {
            var newCentroids = new List<ServiceClusterData>();

            foreach (var cluster in clusters)
            {
                var clusterPoints = cluster.Value;

                if (clusterPoints.Any())
                {
                    var avgLat = clusterPoints.Average(p => p.Latitude);
                    var avgLng = clusterPoints.Average(p => p.Longitude);

                    newCentroids.Add(new ServiceClusterData
                    {
                        Name = "Centroid",
                        Latitude = avgLat,
                        Longitude = avgLng
                    });
                }
                else
                {
                    // If no points are assigned to a centroid, keep it unchanged
                    newCentroids.Add(cluster.Key);
                }
            }

            return newCentroids;
        }

        private bool AreCentroidsConverged(List<ServiceClusterData> newCentroids)
        {
            for (int i = 0; i < Centroids.Count; i++)
            {
                if (CalculateDistance(Centroids[i].Latitude, Centroids[i].Longitude, newCentroids[i].Latitude, newCentroids[i].Longitude) > 0.0001) // Small threshold for convergence
                {
                    return false;
                }
            }

            return true;
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

using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Plan
{
    public int PlanId { get; set; }

    public int TouristId { get; set; }

    public string? Name { get; set; }

    public DateOnly DateBegin { get; set; }

    public DateOnly DateEnd { get; set; }

    public string? Budget { get; set; }

    public string? Preferences { get; set; }

    public string? TripType { get; set; }

    public string? Destination { get; set; }

    public double? TravelTime { get; set; }

    public virtual ICollection<PlanAccommodation> PlanAccommodations { get; set; } = new List<PlanAccommodation>();

    public virtual ICollection<PlanActivity> PlanActivities { get; set; } = new List<PlanActivity>();

    public virtual ICollection<PlanAttraction> PlanAttractions { get; set; } = new List<PlanAttraction>();

    public virtual ICollection<PlanRestaurant> PlanRestaurants { get; set; } = new List<PlanRestaurant>();

    public virtual Tourist Tourist { get; set; } = null!;
}

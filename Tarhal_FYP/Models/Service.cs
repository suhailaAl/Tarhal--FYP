using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string? Type { get; set; }

    public string? Destination { get; set; }

    public int ProviderId { get; set; }

    public virtual Accommodation? Accommodation { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual Attraction? Attraction { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<PlanAccommodation> PlanAccommodations { get; set; } = new List<PlanAccommodation>();

    public virtual ICollection<PlanActivity> PlanActivities { get; set; } = new List<PlanActivity>();

    public virtual ICollection<PlanAttraction> PlanAttractions { get; set; } = new List<PlanAttraction>();

    public virtual ICollection<PlanRestaurant> PlanRestaurants { get; set; } = new List<PlanRestaurant>();

    public virtual ServiceProvider Provider { get; set; } = null!;

    public virtual Restaurant? Restaurant { get; set; }
}

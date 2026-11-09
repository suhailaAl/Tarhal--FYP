using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Restaurant
{
    public int RestaurantId { get; set; }

    public int ProviderId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Url { get; set; } = null!;

    public string Address { get; set; } = null!;

    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public int BookingAvailability { get; set; }

    public string? CuisineType { get; set; }

    public string? PriceRange { get; set; }

    public double Rate { get; set; }

    public int? RatesCount { get; set; }

    public virtual Service RestaurantNavigation { get; set; } = null!;
}

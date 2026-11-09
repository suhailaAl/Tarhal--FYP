using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Accommodation
{
    public int AccommodationId { get; set; }

    public int ProviderId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int PhoneNumber { get; set; }

    public string? PricePerNight { get; set; }

    public string? Description { get; set; }

    public string Amenities { get; set; } = null!;

    public string Address { get; set; } = null!;

    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public int AccommodationAvailable { get; set; }

    public int BookingAvailability { get; set; }

    public string? Url { get; set; }

    public double Rate { get; set; }

    public int? RatesCount { get; set; }

    public virtual Service AccommodationNavigation { get; set; } = null!;
}

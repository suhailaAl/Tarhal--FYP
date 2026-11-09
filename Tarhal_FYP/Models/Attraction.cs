using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Attraction
{
    public int AttractionId { get; set; }

    public int ProviderId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? PopularTimes { get; set; }

    public string? AdmissionFee { get; set; }

    public string? OpeningHours { get; set; }

    public int BookingAvailability { get; set; }

    public string? Address { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? Latitude { get; set; }

    public double? TimeTaken { get; set; }

    public double Rate { get; set; }

    public int? RatesCount { get; set; }

    public virtual Service AttractionNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Activity
{
    public int ActivityId { get; set; }

    public int ProviderId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string Address { get; set; } = null!;

    public string? Type { get; set; }

    public decimal? Price { get; set; }

    public string Category { get; set; } = null!;

    public string? Schedule { get; set; }

    public string? Policies { get; set; }

    public int BookingAvailability { get; set; }

    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public double TimeTaken { get; set; }

    public double? Rate { get; set; }

    public int? RatesCount { get; set; }

    public virtual Service ActivityNavigation { get; set; } = null!;
}

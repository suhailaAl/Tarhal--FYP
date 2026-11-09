using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class PlanAccommodation
{
    public int PlanId { get; set; }

    public int AccommodationId { get; set; }

    public int DayNum { get; set; }

    public virtual Service Accommodation { get; set; } = null!;

    public virtual Plan Plan { get; set; } = null!;
}

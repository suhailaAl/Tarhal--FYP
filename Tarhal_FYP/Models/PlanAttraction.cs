using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class PlanAttraction
{
    public int PlanId { get; set; }

    public int AttractionId { get; set; }

    public int DayNum { get; set; }

    public virtual Service Attraction { get; set; } = null!;

    public virtual Plan Plan { get; set; } = null!;
}

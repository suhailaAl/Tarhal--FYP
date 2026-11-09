using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class PlanActivity
{
    public int PlanId { get; set; }

    public int ActivityId { get; set; }

    public int DayNum { get; set; }

    public virtual Service Activity { get; set; } = null!;

    public virtual Plan Plan { get; set; } = null!;
}

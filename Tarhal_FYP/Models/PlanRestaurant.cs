using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class PlanRestaurant
{
    public int PlanId { get; set; }

    public int RestaurantId { get; set; }

    public int DayNum { get; set; }

    public int Arrange { get; set; }

    public virtual Plan Plan { get; set; } = null!;

    public virtual Service Restaurant { get; set; } = null!;
}

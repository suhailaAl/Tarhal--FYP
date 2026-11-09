using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Tourist
{
    public int TouristId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? TravelTime { get; set; }

    public virtual ICollection<Plan> Plans { get; set; } = new List<Plan>();

    public virtual User TouristNavigation { get; set; } = null!;
}

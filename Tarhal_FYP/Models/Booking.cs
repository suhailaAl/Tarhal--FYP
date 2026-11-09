using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Booking
{
    public int TouristId { get; set; }

    public int ServiceId { get; set; }

    public virtual Service Service { get; set; } = null!;
}

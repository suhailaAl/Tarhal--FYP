using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class ServiceProvider
{
    public int ProviderId { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual User Provider { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}

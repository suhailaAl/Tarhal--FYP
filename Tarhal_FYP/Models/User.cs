using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public virtual Admin? Admin { get; set; }

    public virtual ServiceProvider? ServiceProvider { get; set; }

    public virtual Tourist? Tourist { get; set; }
}

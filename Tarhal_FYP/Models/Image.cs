using System;
using System.Collections.Generic;

namespace Tarhal_FYP.Models;

public partial class Image
{
    public int ImageId { get; set; }

    public int? ServiceId { get; set; }

    public byte[]? ImageData { get; set; }

    public virtual Service? Service { get; set; }
}

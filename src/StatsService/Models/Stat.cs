using System;
using System.Collections.Generic;

namespace StatsService.Models;

public partial class Stat
{
    public int Id { get; set; }

    public string Method { get; set; } = null!;

    public string Path { get; set; } = null!;

    public int StatusCode { get; set; }

    public long DurationMs { get; set; }

    public DateTime Timestamp { get; set; }
}

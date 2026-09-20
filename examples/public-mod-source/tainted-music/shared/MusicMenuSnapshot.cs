using System;

namespace TaintedMusic;

internal sealed class MusicMenuSnapshot
{
    internal DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    internal string StatusLevel { get; set; } = "Info";

    internal string Summary { get; set; } = string.Empty;

    internal string Detail { get; set; } = string.Empty;

    internal string[] Lines { get; set; } = Array.Empty<string>();
}

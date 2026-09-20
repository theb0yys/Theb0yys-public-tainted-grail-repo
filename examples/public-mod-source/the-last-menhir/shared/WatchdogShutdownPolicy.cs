using System;

namespace AvalonExceptions;

internal static class WatchdogShutdownPolicy
{
    internal static bool ShouldPreserveUncleanSession(string currentSessionId, string watchdogSessionId)
    {
        return !string.IsNullOrWhiteSpace(currentSessionId)
            && string.Equals(currentSessionId, watchdogSessionId, StringComparison.OrdinalIgnoreCase);
    }
}

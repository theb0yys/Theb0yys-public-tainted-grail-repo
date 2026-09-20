using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedMusic;

internal static class Feature
{
    internal const string SourceFamily = "tainted-music";

    internal static bool ShouldSuppressNativeMusic(
        bool enabled,
        bool customTrackPlaying,
        bool nativeEventInOwnedScope,
        bool customPlaybackHealthy)
        => enabled && customTrackPlaying && nativeEventInOwnedScope && customPlaybackHealthy;

    internal static bool ShouldRunNativeMusic(
        bool enabled,
        bool customTrackPlaying,
        bool nativeEventInOwnedScope,
        bool customPlaybackHealthy)
        => !ShouldSuppressNativeMusic(
            enabled,
            customTrackPlaying,
            nativeEventInOwnedScope,
            customPlaybackHealthy);

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Music starter initialized. runtime=" + runtimeKind +
           "; native-suppression-requires-healthy-custom-playback";
}

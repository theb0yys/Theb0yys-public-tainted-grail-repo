namespace Tainted.Abstractions.Runtime;

public interface ITaintedRuntimeInfo
{
    string ProcessName { get; }

    TaintedRuntimeKind RuntimeKind { get; }

    string GameVersion { get; }

    string LoaderVersion { get; }

    string UnityVersion { get; }

    string BuildFingerprint { get; }

    TaintedSupportState SupportState { get; }

    bool IsVerifiedBuild { get; }

    bool IsInteropFresh { get; }

    bool WouldMutateRuntime { get; }
}

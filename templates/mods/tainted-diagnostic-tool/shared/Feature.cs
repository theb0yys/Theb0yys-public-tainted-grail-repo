using System.Collections.Generic;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedDiagnosticTool;

internal sealed class BoundedDiagnosticBuffer
{
    private readonly int _capacity;
    private readonly Queue<string> _rows = new Queue<string>();

    internal BoundedDiagnosticBuffer(int capacity)
    {
        _capacity = capacity < 1 ? 1 : capacity;
    }

    internal void Add(string row)
    {
        _rows.Enqueue(row ?? string.Empty);
        while (_rows.Count > _capacity)
            _rows.Dequeue();
    }

    internal string[] Snapshot() => _rows.ToArray();
}

internal static class Feature
{
    internal const string SourceFamily = "Tainted-Diagnostic Tool";

    internal static bool CanWriteDiagnosticFile(
        bool explicitExportRequested,
        bool destinationApproved,
        bool dataRedacted)
        => explicitExportRequested && destinationApproved && dataRedacted;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Diagnostic Tool starter initialized. runtime=" + runtimeKind +
           "; bounded-read-only-diagnostics-first";
}

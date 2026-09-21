using System;
using System.Collections.Generic;
using System.Linq;
using Awaken.TG.Main.Locations;

namespace AvalonCompanions.Patches;

internal static partial class PetCompanionController
{
    private static readonly Dictionary<string, ExternalDialogueCommand> ExternalDialogueCommands =
        new Dictionary<string, ExternalDialogueCommand>(StringComparer.OrdinalIgnoreCase);

    internal static bool RegisterExternalDialogueCommand(
        string ownerId,
        string commandId,
        string targetTemplateGuid,
        string label,
        Func<bool> canExecute,
        Action execute)
    {
        if (string.IsNullOrWhiteSpace(ownerId)
            || string.IsNullOrWhiteSpace(commandId)
            || string.IsNullOrWhiteSpace(targetTemplateGuid)
            || string.IsNullOrWhiteSpace(label)
            || canExecute == null
            || execute == null)
        {
            return false;
        }

        string key = GetExternalDialogueCommandKey(ownerId, commandId);
        ExternalDialogueCommands[key] = new ExternalDialogueCommand(
            ownerId.Trim(),
            commandId.Trim(),
            targetTemplateGuid.Trim(),
            label.Trim(),
            canExecute,
            execute);

        if (_dialogueVisible)
        {
            DestroyDialogueUiHost();
        }

        _logger?.LogInfo(
            $"Avalon Companions registered external dialogue command. owner={ownerId.Trim()}; command={commandId.Trim()}; target={targetTemplateGuid.Trim()}; label={label.Trim()}");
        return true;
    }

    internal static bool UnregisterExternalDialogueCommands(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return false;
        }

        string normalizedOwner = ownerId.Trim();
        string[] keys = ExternalDialogueCommands
            .Where(pair => string.Equals(pair.Value.OwnerId, normalizedOwner, StringComparison.OrdinalIgnoreCase))
            .Select(pair => pair.Key)
            .ToArray();

        foreach (string key in keys)
        {
            ExternalDialogueCommands.Remove(key);
        }

        if (keys.Length > 0 && _dialogueVisible)
        {
            DestroyDialogueUiHost();
        }

        if (keys.Length > 0)
        {
            _logger?.LogInfo($"Avalon Companions unregistered {keys.Length} external dialogue command(s). owner={normalizedOwner}");
        }

        return keys.Length > 0;
    }

    private static List<ExternalDialogueCommand> GetExternalDialogueCommands(Location? location)
    {
        string targetGuid = location?.Template?.GUID ?? string.Empty;
        if (string.IsNullOrWhiteSpace(targetGuid))
        {
            return new List<ExternalDialogueCommand>();
        }

        return ExternalDialogueCommands.Values
            .Where(command => string.Equals(command.TargetTemplateGuid, targetGuid, StringComparison.OrdinalIgnoreCase))
            .OrderBy(command => command.OwnerId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(command => command.CommandId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetExternalDialogueCommandKey(string ownerId, string commandId)
    {
        return ownerId.Trim() + ":" + commandId.Trim();
    }

    private static void ClearExternalDialogueCommands()
    {
        ExternalDialogueCommands.Clear();
    }

    private sealed class ExternalDialogueCommand
    {
        internal ExternalDialogueCommand(
            string ownerId,
            string commandId,
            string targetTemplateGuid,
            string label,
            Func<bool> canExecute,
            Action execute)
        {
            OwnerId = ownerId;
            CommandId = commandId;
            TargetTemplateGuid = targetTemplateGuid;
            Label = label;
            CanExecute = canExecute;
            Execute = execute;
        }

        internal string OwnerId { get; }
        internal string CommandId { get; }
        internal string TargetTemplateGuid { get; }
        internal string Label { get; }
        internal Func<bool> CanExecute { get; }
        internal Action Execute { get; }
    }
}

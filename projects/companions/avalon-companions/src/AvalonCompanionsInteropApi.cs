using System;
using AvalonCompanions.Patches;

namespace AvalonCompanions;

public static class AvalonCompanionsInteropApi
{
    public const int ApiVersion = 1;

    public static bool RegisterDialogueCommand(
        string ownerId,
        string commandId,
        string targetTemplateGuid,
        string label,
        Func<bool> canExecute,
        Action execute)
    {
        return PetCompanionController.RegisterExternalDialogueCommand(
            ownerId,
            commandId,
            targetTemplateGuid,
            label,
            canExecute,
            execute);
    }

    public static bool UnregisterDialogueCommands(string ownerId)
    {
        return PetCompanionController.UnregisterExternalDialogueCommands(ownerId);
    }
}

using System;

namespace AvalonCompanions.Framework;

internal static class ReviewedNativeDefendAnimalPolicy
{
    internal const string WolfTemplateGuid = "9086dee514edc644b9b55890d885db3f";
    internal const string BearTemplateGuid = "c45508309b84907429f83d1361918fc2";

    internal static bool IsApproved(string templateGuid)
    {
        return string.Equals(templateGuid, WolfTemplateGuid, StringComparison.Ordinal)
            || string.Equals(templateGuid, BearTemplateGuid, StringComparison.Ordinal);
    }
}

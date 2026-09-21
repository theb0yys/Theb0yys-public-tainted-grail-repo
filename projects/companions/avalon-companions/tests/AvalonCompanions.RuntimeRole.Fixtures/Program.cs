using System;
using AvalonCompanions.Framework;

Check(
    ReviewedNativeDefendAnimalPolicy.IsApproved(ReviewedNativeDefendAnimalPolicy.WolfTemplateGuid),
    "exact-reviewed-wolf-has-native-defend-capability");
Check(
    ReviewedNativeDefendAnimalPolicy.IsApproved(ReviewedNativeDefendAnimalPolicy.BearTemplateGuid),
    "exact-reviewed-bear-has-native-defend-capability");
Check(
    !ReviewedNativeDefendAnimalPolicy.IsApproved("63fa2f502163174468799959dd025319"),
    "interaction-only-bear-remains-passive");
Check(
    !ReviewedNativeDefendAnimalPolicy.IsApproved("467a9c9208854394cbb77032251288a1"),
    "deer-remains-passive");
Check(
    !ReviewedNativeDefendAnimalPolicy.IsApproved(string.Empty),
    "empty-template-guid-remains-passive");

Console.WriteLine("PASS Avalon Companions reviewed native-defend animal fixtures (5 checks).");

static void Check(bool condition, string name)
{
    if (!condition)
    {
        throw new InvalidOperationException("FAIL " + name);
    }

    Console.WriteLine("PASS " + name);
}

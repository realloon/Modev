using JetBrains.Annotations;
using HarmonyLib;
using Modev.Utility;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.GetWorkshopUploadDirectory))]
public static class Postfix_ModMetaData_GetWorkshopUploadDirectory {
    [UsedImplicitly]
    public static void Postfix(ref DirectoryInfo __result) {
        var settings = ModevMod.Settings;
        var excludedRules = settings.GetExcludedRules();
        var ignoreDotPrefixedPaths = settings.IgnoreDotPrefixedPaths;

        if (excludedRules.Count == 0 && !ignoreDotPrefixedPaths) return;

        __result = UploadContentFilter.BuildFilteredCopy(__result, excludedRules, ignoreDotPrefixedPaths);
    }
}
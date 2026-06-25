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
        var bundleDefs = settings.BundleDefs;

        if (excludedRules.Count == 0 && !ignoreDotPrefixedPaths && !bundleDefs) return;

        __result = UploadContentFilter.BuildFilteredCopy(__result, excludedRules, ignoreDotPrefixedPaths);

        if (bundleDefs) {
            DefBundler.Bundle(__result);
        }
    }
}
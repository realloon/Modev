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
        var excludedFolders = settings.GetExcludedFolders();
        var excludedFiles = settings.GetExcludedFiles();
        var ignoreDotPrefixedPaths = settings.IgnoreDotPrefixedPaths;

        if (excludedFolders.Count == 0
            && excludedFiles.Count == 0
            && !ignoreDotPrefixedPaths) return;

        __result = UploadContentFilter.BuildFilteredCopy(__result, excludedFolders,
            excludedFiles, ignoreDotPrefixedPaths);
    }
}
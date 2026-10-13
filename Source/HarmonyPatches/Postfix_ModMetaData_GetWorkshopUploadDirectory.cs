using JetBrains.Annotations;
using HarmonyLib;
using Modev.Utility;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.GetWorkshopUploadDirectory))]
public static class Postfix_ModMetaData_GetWorkshopUploadDirectory {
    [UsedImplicitly]
    public static void Postfix(ModMetaData __instance, ref DirectoryInfo __result) {
        if (UploadStaging.ContentRoot is not { } root) return;
        if (__instance.workshopUploadDirectoryOverride is not null) return;

        UploadStaging.SyncPublishedFileId(root, __instance);
        __result = root;
    }
}
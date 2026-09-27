using JetBrains.Annotations;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.GetWorkshopPreviewImagePath))]
public static class Postfix_ModMetaData_GetWorkshopPreviewImagePath {
    [UsedImplicitly]
    public static void Postfix(ModMetaData __instance, ref string __result) {
        var webpPath = Path.Combine(__instance.RootDir.FullName, "About", "Preview.webp");
        if (File.Exists(webpPath)) {
            __result = webpPath;
        }
    }
}
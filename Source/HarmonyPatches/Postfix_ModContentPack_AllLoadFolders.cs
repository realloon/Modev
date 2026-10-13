using JetBrains.Annotations;
using HarmonyLib;
using Modev.Utility;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(ModContentPack), nameof(ModContentPack.AllLoadFolders), typeof(ModMetaData))]
public static class Postfix_ModContentPack_AllLoadFolders {
    [UsedImplicitly]
    public static void Postfix(ModMetaData mod, ref List<string> __result) {
        if (UploadStaging.ContentRoot is { } root) {
            __result = ModContentPack.AllLoadFolders(root.FullName, mod.loadFolders);
        }
    }
}
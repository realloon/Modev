using JetBrains.Annotations;
using HarmonyLib;
using UnityStubs;
using Verse.Steam;
using Modev.Utility;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(Workshop), "BuildStagingDirectory")]
public static class Prefix_Workshop_BuildStagingDirectory {
    [UsedImplicitly]
    [HarmonyPrefix]
    public static bool Prefix(DirectoryInfo staging, bool skipSourceTextures) {
        if (UploadStaging.ContentRoot is not { } source) return true;

        if (staging.Exists) staging.Delete(true);
        staging.Create();

        foreach (var file in Directory.EnumerateFiles(source.FullName, "*", SearchOption.AllDirectories)) {
            if (ContentPacks.IsPackSidecar(file)) continue;
            if (skipSourceTextures && ContentPacks.IsPackedSource(file)) continue;

            var target = Path.Combine(staging.FullName, Path.GetRelativePath(source.FullName, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        return false;
    }
}
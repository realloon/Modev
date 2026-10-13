using HarmonyLib;
using Verse.Steam;

namespace Modev.Utility;

public static class UploadStaging {
    public static DirectoryInfo? ContentRoot { get; private set; }

    public static bool Prepare(ModMetaData mod) {
        var settings = ModevMod.Settings;

        if (!settings.HasContentTransforms) {
            ContentRoot = null;
            return true;
        }

        if (AccessTools.Method(typeof(Workshop), "BuildStagingDirectory") is null ||
            AccessTools.Method(typeof(ModContentPack), nameof(ModContentPack.AllLoadFolders),
                [typeof(ModMetaData)]) is null) {
            Log.Error("Modev: the game's upload pipeline has changed, so content filters cannot be applied. Upload cancelled.");
            ContentRoot = null;
            return false;
        }

        var staging = UploadContentFilter.BuildFilteredCopy(mod.RootDir,
            settings.ExcludedRules, settings.IgnoreDotPrefixedPaths);

        if (settings.BundleDefs) DefBundler.Bundle(staging);
        if (settings.StripXmlComments) XmlCommentStripper.StripDirectory(staging);

        ContentRoot = staging;
        return true;
    }

    public static void SyncPublishedFileId(DirectoryInfo root, ModMetaData mod) {
        var source = Path.Combine(mod.RootDir.FullName, "About", "PublishedFileId.txt");
        if (!File.Exists(source)) return;

        var target = Path.Combine(root.FullName, "About", "PublishedFileId.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, true);
    }
}
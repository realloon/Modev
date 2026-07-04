using JetBrains.Annotations;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse.Sound;
using Verse.Steam;
using Modev.Utility;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_ConfirmModUpload))]
[HarmonyPatch(MethodType.Constructor, typeof(ModMetaData), typeof(Action))]
public static class Postfix_Dialog_ConfirmModUpload_Ctor {
    private static readonly string ConfirmContentAuthorText = "ConfirmContentAuthor".Translate();
    private static readonly MethodInfo UploadMethod = AccessTools.Method(typeof(Workshop), "Upload")!;

    [UsedImplicitly]
    public static void Postfix(Dialog_ConfirmModUpload __instance, ModMetaData mod) {
        __instance.text = __instance.text + "\n\n\n" + ConfirmContentAuthorText;
        __instance.buttonAAction = () => OpenUploadPreview(mod);
        __instance.acceptAction = __instance.buttonAAction;
    }

    private static void OpenUploadPreview(ModMetaData mod) {
        SoundDefOf.Tick_High.PlayOneShotOnCamera();

        var settings = ModevMod.Settings;

        var interactionDelay = settings.SkipWorkshopConfirmDelay ? 0f : 6f;

        Find.WindowStack.Add(new Dialog_UploadPreview(BuildUploadPreviewText(mod, settings),
            interactionDelay, () => {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                #if DEBUG
                DebugExport.ExportToDesktop(mod);
                #else
                UploadMethod.Invoke(null, [mod]);
                #endif
            }));
    }

    private static string BuildUploadPreviewText(ModMetaData mod, ModevSettings settings) {
        var includedPaths = UploadContentFilter.ListIncludedTopLevelPaths(mod.RootDir,
            settings.GetExcludedRules(), settings.IgnoreDotPrefixedPaths);

        return includedPaths.Empty()
            ? "Modev_UploadPreview_Empty".Translate()
            : string.Join("\n", includedPaths);
    }
}
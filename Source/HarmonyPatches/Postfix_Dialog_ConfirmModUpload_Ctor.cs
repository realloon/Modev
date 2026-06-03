using System.Reflection;
using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using Modev.Utility;
using Verse.Sound;
using Verse.Steam;

// ReSharper disable InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_ConfirmModUpload))]
[HarmonyPatch(MethodType.Constructor, typeof(ModMetaData), typeof(Action))]
public static class Postfix_Dialog_ConfirmModUpload_Ctor {
    private static readonly string ConfirmContentAuthorText = "ConfirmContentAuthor".Translate();
    private static readonly MethodInfo UploadMethod = AccessTools.Method(typeof(Workshop), "Upload")!;

    [UsedImplicitly]
    public static void Postfix(Dialog_ConfirmModUpload __instance, ModMetaData mod) {
        __instance.text = __instance.text + "\n\n" + ConfirmContentAuthorText;
        __instance.buttonAAction = () => OpenUploadPreview(mod);
        __instance.acceptAction = __instance.buttonAAction;
    }

    private static void OpenUploadPreview(ModMetaData mod) {
        SoundDefOf.Tick_High.PlayOneShotOnCamera();

        var settings = ModevMod.Settings;
        var uploadAction = () => {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            UploadMethod.Invoke(null, [mod]);
        };

        var interactionDelay = settings.SkipWorkshopConfirmDelay ? 0f : 6f;

        Find.WindowStack.Add(new Dialog_UploadPreview("Modev_UploadPreview_Title".Translate(),
            BuildUploadPreviewText(mod, settings),
            "Yes".Translate(), uploadAction, "No".Translate(), null,
            true, uploadAction, delegate { }, WindowLayer.Dialog, interactionDelay));
    }

    private static string BuildUploadPreviewText(ModMetaData mod, ModevSettings settings) {
        var includedPaths = UploadContentFilter.ListIncludedTopLevelPaths(mod.RootDir,
            settings.GetExcludedRules(), settings.IgnoreDotPrefixedPaths);

        return includedPaths.Empty()
            ? "Modev_UploadPreview_Empty".Translate()
            : string.Join("\n", includedPaths);
    }
}
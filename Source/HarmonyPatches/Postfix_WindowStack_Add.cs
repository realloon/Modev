using JetBrains.Annotations;
using HarmonyLib;
using Modev.Utility;

// ReSharper disable once InconsistentNaming

namespace Modev.HarmonyPatches;

[HarmonyPatch(typeof(WindowStack), nameof(WindowStack.Add))]
public static class Postfix_WindowStack_Add {
    private static readonly string ConfirmContentAuthorText = "ConfirmContentAuthor".Translate();

    [UsedImplicitly]
    public static void Postfix(Window window) {
        if (window is Dialog_UploadPreview) return;
        if (window is not Dialog_MessageBox dialog) return;

        var settings = ModevMod.Settings;

        string text = dialog.text;
        if (text.Equals("ConfirmSteamWorkshopUpload".Translate(), StringComparison.Ordinal)) {
            dialog.text = text + "\n\n" + ConfirmContentAuthorText;
            return;
        }

        if (!text.Equals(ConfirmContentAuthorText, StringComparison.Ordinal) &&
            text.IndexOf("Did you create this content yourself", StringComparison.OrdinalIgnoreCase) < 0) {
            return;
        }

        if (settings.SkipWorkshopConfirmDelay && dialog.interactionDelay > 0f) {
            dialog.interactionDelay = 0f;
        }

        if (!TryGetUploadingMod(dialog, out var mod)) return;
        var replacement = new Dialog_UploadPreview("Modev_UploadPreview_Title".Translate(),
            BuildUploadPreviewText(mod, settings),
            dialog.buttonAText, dialog.buttonAAction, dialog.buttonBText, dialog.buttonBAction,
            dialog.buttonADestructive,
            dialog.acceptAction, dialog.cancelAction, dialog.layer, dialog.interactionDelay);
        Find.WindowStack.TryRemove(dialog, false);
        Find.WindowStack.Add(replacement);
    }

    private static string BuildUploadPreviewText(ModMetaData mod, ModevSettings settings) {
        var includedPaths = UploadContentFilter.ListIncludedTopLevelPaths(mod.RootDir,
            settings.GetExcludedFolders(), settings.GetExcludedFiles(), settings.IgnoreDotPrefixedPaths);

        return includedPaths.Empty()
            ? "Modev_UploadPreview_Empty".Translate()
            : string.Join("\n", includedPaths);
    }

    private static bool TryGetUploadingMod(Dialog_MessageBox dialog, out ModMetaData mod) {
        return TryGetModFromDelegate(dialog.buttonAAction, out mod) ||
               TryGetModFromDelegate(dialog.acceptAction, out mod);
    }

    private static bool TryGetModFromDelegate(Delegate? callback, out ModMetaData mod) {
        if (callback?.Target == null) {
            mod = null!;
            return false;
        }

        foreach (var field in AccessTools.GetDeclaredFields(callback.Target.GetType())) {
            if (field.GetValue(callback.Target) is not ModMetaData foundMod) {
                continue;
            }

            mod = foundMod;
            return true;
        }

        mod = null!;
        return false;
    }
}
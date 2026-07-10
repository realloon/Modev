using JetBrains.Annotations;
using UnityEngine;

namespace Modev;

[UsedImplicitly]
public sealed class ModevMod : Mod {
    public static ModevSettings Settings = null!;
    private const float ActionButtonSize = 30f;
    private const float ActionButtonGap = 8f;
    private const float ActionRightPadding = 16f;
    private string _pendingExcludedRule = string.Empty;
    private Vector2 _excludedRulesScrollPosition = Vector2.zero;

    public ModevMod(ModContentPack content) : base(content) {
        Settings = GetSettings<ModevSettings>();
    }

    public override string SettingsCategory() => "Modev";

    public override void DoSettingsWindowContents(Rect inRect) {
        var y = inRect.y;
        var skipDelay = Settings.SkipWorkshopConfirmDelay;
        var skipDelayRect = new Rect(inRect.x, y, inRect.width, 30f);
        if (Mouse.IsOver(skipDelayRect)) {
            Widgets.DrawHighlight(skipDelayRect);
        }

        Widgets.CheckboxLabeled(skipDelayRect, "Modev_SkipDelay".Translate(), ref skipDelay);
        if (skipDelay != Settings.SkipWorkshopConfirmDelay) {
            Settings.SkipWorkshopConfirmDelay = skipDelay;
            WriteSettings();
        }

        y += 32f;
        var ignoreDotPrefixedPaths = Settings.IgnoreDotPrefixedPaths;
        var ignoreDotPrefixedPathsRect = new Rect(inRect.x, y, inRect.width, 30f);
        if (Mouse.IsOver(ignoreDotPrefixedPathsRect)) {
            Widgets.DrawHighlight(ignoreDotPrefixedPathsRect);
        }

        Widgets.CheckboxLabeled(ignoreDotPrefixedPathsRect, "Modev_IgnoreDotPrefixedPaths".Translate(),
            ref ignoreDotPrefixedPaths);
        if (ignoreDotPrefixedPaths != Settings.IgnoreDotPrefixedPaths) {
            Settings.IgnoreDotPrefixedPaths = ignoreDotPrefixedPaths;
            WriteSettings();
        }

        y += 32f;
        var bundleDefs = Settings.BundleDefs;
        var bundleDefsRect = new Rect(inRect.x, y, inRect.width, 30f);
        if (Mouse.IsOver(bundleDefsRect)) {
            Widgets.DrawHighlight(bundleDefsRect);
        }

        Widgets.CheckboxLabeled(bundleDefsRect, "Modev_BundleDefs".Translate(), ref bundleDefs);
        if (bundleDefs != Settings.BundleDefs) {
            Settings.BundleDefs = bundleDefs;
            WriteSettings();
        }

        y += 40f;
        Widgets.DrawBoxSolid(new Rect(inRect.x, y, inRect.width, 1f), new Color(1f, 1f, 1f, 0.24f));
        y += 16f;

        Widgets.Label(new Rect(inRect.x, y, inRect.width, 28f), "Modev_ExcludeRules_Label".Translate());
        y += 32f;

        var inputRect = new Rect(inRect.x, y, inRect.width - ActionButtonSize - ActionButtonGap - ActionRightPadding,
            30f);
        GUI.SetNextControlName("Modev_ExcludedRuleInput");
        _pendingExcludedRule = Widgets.TextField(inputRect, _pendingExcludedRule);

        var addButtonRect = new Rect(inRect.x + inRect.width - ActionButtonSize - ActionRightPadding, y,
            ActionButtonSize, ActionButtonSize);
        var addButtonColor = _pendingExcludedRule.IsWhiteSpace() ? Color.gray : Color.white;
        if (Widgets.ButtonImage(addButtonRect, TexButton.Add, addButtonColor)) {
            TryAddPendingExcludedRule();
        }

        if (GUI.GetNameOfFocusedControl() == "Modev_ExcludedRuleInput" &&
            Event.current.type == EventType.KeyDown &&
            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
            TryAddPendingExcludedRule();
            Event.current.Use();
        }

        y += 40f;
        var rulesListRect = new Rect(inRect.x, y, inRect.width, inRect.yMax - y);
        DrawExcludedRulesList(rulesListRect);
    }

    private void DrawExcludedRulesList(Rect listRect) {
        var excludedRules = Settings.GetExcludedRules();
        var contentHeight = Mathf.Max(listRect.height, excludedRules.Count * 34f + 8f);
        var viewRect = new Rect(0f, 0f, listRect.width - 16f, contentHeight);
        Widgets.BeginScrollView(listRect, ref _excludedRulesScrollPosition, viewRect);

        if (excludedRules.Count == 0) {
            Widgets.Label(new Rect(0f, 0f, viewRect.width, 24f), "Modev_ExcludeRules_Empty".Translate());
            Widgets.EndScrollView();
            return;
        }

        var removeIndex = -1;
        for (var i = 0; i < excludedRules.Count; i++) {
            var rowY = i * 34f;
            var rowRect = new Rect(0f, rowY, viewRect.width, 30f);
            if (Mouse.IsOver(rowRect)) {
                Widgets.DrawHighlight(rowRect);
            }

            var textRect = new Rect(rowRect.x + 4f, rowRect.y + 4f,
                rowRect.width - ActionButtonSize - ActionButtonGap - 4f, 24f);
            Widgets.Label(textRect, excludedRules[i]);

            if (!Mouse.IsOver(rowRect)) continue;

            var removeButtonRect = new Rect(rowRect.xMax - ActionButtonSize, rowRect.y, ActionButtonSize,
                ActionButtonSize);
            if (Widgets.ButtonImage(removeButtonRect, TexButton.Delete)) {
                removeIndex = i;
            }
        }

        Widgets.EndScrollView();

        if (removeIndex < 0) return;

        Settings.RemoveExcludedRuleAt(removeIndex);
        WriteSettings();
    }

    private void TryAddPendingExcludedRule() {
        if (!Settings.TryAddExcludedRule(_pendingExcludedRule)) return;

        _pendingExcludedRule = string.Empty;
        WriteSettings();
    }
}
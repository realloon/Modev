using UnityEngine;

namespace Modev;

// ReSharper disable once InconsistentNaming
public sealed class Dialog_UploadPreview : Dialog_MessageBox {
    private static readonly string[] MonospaceFontNames = [
        "Menlo",
        "Consolas"
    ];

    private static Font? _monospaceFont;
    private readonly string _contentText;
    private Vector2 _scrollPosition = Vector2.zero;

    public override Vector2 InitialSize => new(560f, 400f);

    private float TimeUntilInteractive => interactionDelay - (Time.realtimeSinceStartup - field);
    private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

    public Dialog_UploadPreview(string title, string contentText, string buttonAText, Action? buttonAAction,
        string? buttonBText, Action? buttonBAction, bool buttonADestructive, Action? acceptAction,
        Action? cancelAction, WindowLayer layer, float interactionDelay)
        : base(string.Empty, buttonAText, buttonAAction, buttonBText, buttonBAction, title, buttonADestructive,
            acceptAction, cancelAction, layer) {
        _contentText = contentText;
        this.interactionDelay = interactionDelay;
        TimeUntilInteractive = RealTime.LastRealTime;
    }

    public override void DoWindowContents(Rect inRect) {
        var y = inRect.y;
        if (!title.NullOrEmpty()) {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, y, inRect.width, 42f), title);
            y += 42f;
        }

        Text.Font = GameFont.Small;
        var bodyRect = new Rect(inRect.x, y, inRect.width, inRect.height - 35f - 5f - y);
        var bodyWidth = bodyRect.width - 16f;
        var bodyStyle = CreateBodyStyle();
        var contentHeight = bodyStyle.CalcHeight(new GUIContent(_contentText), bodyWidth);
        var viewRect = new Rect(0f, 0f, bodyWidth, contentHeight);
        Widgets.BeginScrollView(bodyRect, ref _scrollPosition, viewRect);
        GUI.Label(new Rect(0f, 0f, viewRect.width, viewRect.height), _contentText, bodyStyle);
        Widgets.EndScrollView();

        DrawButtons(inRect);
    }

    public override void OnAcceptKeyPressed() {
        if (!InteractionDelayExpired) {
            return;
        }

        base.OnAcceptKeyPressed();
    }

    private static GUIStyle CreateBodyStyle() {
        _monospaceFont ??= Font.CreateDynamicFontFromOSFont(MonospaceFontNames, 14);
        return new GUIStyle(Text.CurFontStyle) {
            font = _monospaceFont ?? Text.CurFontStyle.font,
            fontSize = 14,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true
        };
    }

    private void DrawButtons(Rect inRect) {
        var buttonCount = buttonCText.NullOrEmpty() ? 2 : 3;
        var buttonWidth = inRect.width / buttonCount;
        var actualButtonWidth = buttonWidth - 10f;

        if (buttonADestructive) {
            GUI.color = new Color(1f, 0.3f, 0.35f);
        }

        var primaryLabel = InteractionDelayExpired
            ? buttonAText
            : buttonAText + "(" + Mathf.Ceil(TimeUntilInteractive).ToString("F0") + ")";

        if (Widgets.ButtonText(
                new Rect(buttonWidth * (buttonCount - 1) + 10f, inRect.height - 35f, actualButtonWidth, 35f),
                primaryLabel) && InteractionDelayExpired) {
            buttonAAction?.Invoke();
            Close();
        }

        GUI.color = Color.white;
        if (buttonBText != null &&
            Widgets.ButtonText(new Rect(0f, inRect.height - 35f, actualButtonWidth, 35f), buttonBText)) {
            buttonBAction?.Invoke();
            Close();
        }

        if (buttonCText == null || !Widgets.ButtonText(
                new Rect(buttonWidth, inRect.height - 35f, actualButtonWidth, 35f),
                buttonCText)) return;

        buttonCAction?.Invoke();

        if (buttonCClose) {
            Close();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ColonistTitles;

public static class LabelDrawer
{
    public const float LineHeight = GenMapUI.NameBGHeight_Small;

    private const float BackgroundPadding = GenMapUI.NameBGExtraWidth_Small;

    // Height of the vanilla name label this mod draws under.
    public static float NameHeight => Prefs.DisableTinyText ? GenMapUI.NameBGHeight_Small : GenMapUI.NameBGHeight_Tiny;

    /// <summary>Draws up to maxLines lines centered on x, the first one's top at y.</summary>
    public static void DrawLines(IReadOnlyList<TitleLine> lines, float x, float y, float maxWidth, float alpha, int maxLines = int.MaxValue)
    {
        int count = Mathf.Min(lines.Count, maxLines);
        if (count <= 0)
        {
            return;
        }

        ColonistTitlesSettings settings = ColonistTitlesMod.Settings;
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        Color oldColor = GUI.color;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperCenter;
        float opacity = settings.Opacity * alpha;

        for (int i = 0; i < count; i++)
        {
            string text = lines[i].Shown(maxWidth, out float width);
            Rect background = new Rect(x - width / 2f - BackgroundPadding, y + i * LineHeight, width + BackgroundPadding * 2f, LineHeight);
            if (settings.DrawBackground)
            {
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(background, TexUI.GrayTextBG);
            }

            Color color = lines[i].Color;
            color.a = opacity;
            GUI.color = color;
            Widgets.Label(new Rect(x - width / 2f, background.y - 2f, width, 100f), text);
        }

        GUI.color = oldColor;
        Text.Anchor = oldAnchor;
        Text.Font = oldFont;
    }
}

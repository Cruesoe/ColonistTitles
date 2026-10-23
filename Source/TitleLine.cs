using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>One label line, with its truncated text and width cached for the last width it was drawn at.</summary>
public sealed class TitleLine
{
    public readonly string Text;
    public readonly Color Color;

    private float measuredFor = -1f;
    private string shown = "";
    private float width;

    public TitleLine(string text, Color color)
    {
        Text = text;
        Color = color;
    }

    // Truncates and measures with the current font; recomputed only when maxWidth changes.
    public string Shown(float maxWidth, out float shownWidth)
    {
        if (maxWidth != measuredFor)
        {
            measuredFor = maxWidth;
            shown = Text.Truncate(maxWidth);
            width = Verse.Text.CalcSize(shown).x;
        }

        shownWidth = width;
        return shown;
    }
}

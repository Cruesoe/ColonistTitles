using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Draws title lines under each name on the colonist bar.</summary>
[HarmonyPatch(typeof(ColonistBarColonistDrawer), nameof(ColonistBarColonistDrawer.DrawColonist))]
public static class Patch_ColonistBarColonistDrawer_DrawColonist
{
    public static void Postfix(Rect rect, Pawn colonist, Map pawnMap, bool reordering)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        ColonistBar bar = Find.ColonistBar;
        float? nextRowTop = BarRows.NextRowTop(bar.DrawLocs, rect.y);
        if (ColonistPortraitCompat.BarOn)
        {
            // The themed card holds Useful Marks' title in its name, so lines start right under that.
            if (colonist != null)
            {
                DrawLines(rect, colonist, pawnMap, ColonistPortraitCompat.LinesTop(rect, colonist), ColonistPortraitCompat.CardGap(bar.Scale), bar.GetEntryRectAlpha(rect), reordering, nextRowTop, otherModOffset: false);
            }

            return;
        }

        DrawBarLines(rect, colonist, pawnMap, bar.Scale, bar.SpaceBetweenColonistsHorizontal, bar.GetEntryRectAlpha(rect), reordering, nextRowTop, otherModLabels: true);
    }

    // Shared with colonist bars that replace the vanilla one, for repaint events only. Lines stop before nextRowTop, when there is a row below.
    public static void DrawBarLines(Rect rect, Pawn colonist, Map pawnMap, float scale, float spaceBetween, float alpha, bool reordering, float? nextRowTop, bool otherModLabels)
    {
        if (colonist == null)
        {
            return;
        }

        DrawLines(rect, colonist, pawnMap, rect.yMax - 4f * scale + LabelDrawer.NameHeight, spaceBetween, alpha, reordering, nextRowTop, otherModLabels);
    }

    // nameBottom is where the name label ends; other mods' labels under it and the bar offset setting are added here.
    private static void DrawLines(Rect rect, Pawn colonist, Map pawnMap, float nameBottom, float spaceBetween, float alpha, bool reordering, float? nextRowTop, bool otherModOffset)
    {
        var lines = TitleLabels.For(colonist, forMap: false, out float offset);
        if (lines.Count == 0)
        {
            return;
        }

        // Matches the dimming vanilla gives names of pawns on other maps.
        if (pawnMap == null ? !WorldRendererUtility.WorldSelected : pawnMap != Find.CurrentMap || WorldRendererUtility.WorldSelected)
        {
            alpha = Mathf.Min(alpha, 0.4f);
        }

        // Matches the dimming vanilla gives a colonist being dragged.
        if (reordering)
        {
            alpha *= 0.5f;
        }

        float top = nameBottom + (otherModOffset ? offset : 0f) + ColonistTitlesMod.Settings.BarOffset;
        int maxLines = nextRowTop.HasValue ? Mathf.FloorToInt((nextRowTop.Value - top) / LabelDrawer.ScaledLineHeight) : int.MaxValue;
        LabelDrawer.DrawLines(lines, rect.center.x, top, rect.width + spaceBetween - 2f, alpha, maxLines);
    }
}

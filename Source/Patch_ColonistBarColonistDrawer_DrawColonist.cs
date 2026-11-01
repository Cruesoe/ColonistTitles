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
        DrawBarLines(rect, colonist, pawnMap, bar.Scale, bar.SpaceBetweenColonistsHorizontal, bar.GetEntryRectAlpha(rect), reordering, BarRows.NextRowTop(bar.DrawLocs, rect.y), otherModLabels: true);
    }

    // Shared with colonist bars that replace the vanilla one, for repaint events only. Lines stop before nextRowTop, when there is a row below.
    public static void DrawBarLines(Rect rect, Pawn colonist, Map pawnMap, float scale, float spaceBetween, float alpha, bool reordering, float? nextRowTop, bool otherModLabels)
    {
        if (colonist == null)
        {
            return;
        }

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

        float top = rect.yMax - 4f * scale + LabelDrawer.NameHeight + (otherModLabels ? offset : 0f);
        int maxLines = nextRowTop.HasValue ? Mathf.FloorToInt((nextRowTop.Value - top) / LabelDrawer.ScaledLineHeight) : int.MaxValue;
        LabelDrawer.DrawLines(lines, rect.center.x, top, rect.width + spaceBetween - 2f, alpha, maxLines);
    }
}

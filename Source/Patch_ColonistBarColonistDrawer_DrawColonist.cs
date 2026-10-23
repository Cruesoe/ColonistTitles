using System.Collections.Generic;
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
    private static readonly List<float> rowTops = new List<float>();
    private static int rowTopsFrame = -1;

    public static void Postfix(Rect rect, Pawn colonist, Map pawnMap)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        ColonistBar bar = Find.ColonistBar;
        DrawBarLines(rect, colonist, pawnMap, bar.Scale, bar.SpaceBetweenColonistsHorizontal, bar.GetEntryRectAlpha(rect), NextRowTop(bar, rect.y), otherModLabels: true);
    }

    // Shared with colonist bars that replace the vanilla one. Lines stop before nextRowTop, when there is a row below.
    public static void DrawBarLines(Rect rect, Pawn colonist, Map pawnMap, float scale, float spaceBetween, float alpha, float? nextRowTop, bool otherModLabels)
    {
        if (Event.current.type != EventType.Repaint || colonist == null)
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

        float top = rect.yMax - 4f * scale + LabelDrawer.NameHeight + (otherModLabels ? offset : 0f);
        int maxLines = nextRowTop.HasValue ? Mathf.FloorToInt((nextRowTop.Value - top) / LabelDrawer.LineHeight) : int.MaxValue;
        LabelDrawer.DrawLines(lines, rect.center.x, top, rect.width + spaceBetween - 2f, alpha, maxLines);
    }

    // Top of the first bar row below y, or null on the last row. Row tops are gathered once per frame.
    private static float? NextRowTop(ColonistBar bar, float y)
    {
        if (rowTopsFrame != RealTime.frameCount)
        {
            rowTopsFrame = RealTime.frameCount;
            rowTops.Clear();
            List<Vector2> locs = bar.DrawLocs;
            for (int i = 0; i < locs.Count; i++)
            {
                if (!rowTops.Contains(locs[i].y))
                {
                    rowTops.Add(locs[i].y);
                }
            }
        }

        float? next = null;
        for (int i = 0; i < rowTops.Count; i++)
        {
            if (rowTops[i] > y + 1f && (next == null || rowTops[i] < next.Value))
            {
                next = rowTops[i];
            }
        }

        return next;
    }
}

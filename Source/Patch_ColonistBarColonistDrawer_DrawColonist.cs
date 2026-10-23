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
    public static void Postfix(Rect rect, Pawn colonist, Map pawnMap)
    {
        ColonistBar bar = Find.ColonistBar;
        DrawBarLines(rect, colonist, pawnMap, bar.Scale, bar.SpaceBetweenColonistsHorizontal, bar.GetEntryRectAlpha(rect));
    }

    // Shared with colonist bars that replace the vanilla one.
    public static void DrawBarLines(Rect rect, Pawn colonist, Map pawnMap, float scale, float spaceBetween, float alpha)
    {
        if (Event.current.type != EventType.Repaint || colonist == null)
        {
            return;
        }

        var lines = TitleLabels.For(colonist, forMap: false);
        if (lines.Count == 0)
        {
            return;
        }

        // Matches the dimming vanilla gives names of pawns on other maps.
        if (pawnMap == null ? !WorldRendererUtility.WorldSelected : pawnMap != Find.CurrentMap || WorldRendererUtility.WorldSelected)
        {
            alpha = Mathf.Min(alpha, 0.4f);
        }

        float top = rect.yMax - 4f * scale + LabelDrawer.NameHeight;
        LabelDrawer.DrawLines(lines, rect.center.x, top, rect.width + spaceBetween - 2f, alpha);
    }
}

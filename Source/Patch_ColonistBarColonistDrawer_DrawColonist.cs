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
        if (Event.current.type != EventType.Repaint || colonist == null)
        {
            return;
        }

        ColonistBar bar = Find.ColonistBar;
        var lines = TitleLabels.For(colonist, forMap: false);
        if (lines.Count == 0)
        {
            return;
        }

        // Matches the dimming vanilla gives names of pawns on other maps.
        float alpha = bar.GetEntryRectAlpha(rect);
        if (pawnMap == null ? !WorldRendererUtility.WorldSelected : pawnMap != Find.CurrentMap || WorldRendererUtility.WorldSelected)
        {
            alpha = Mathf.Min(alpha, 0.4f);
        }

        float top = rect.yMax - 4f * bar.Scale + LabelDrawer.NameHeight;
        LabelDrawer.DrawLines(lines, rect.center.x, top, rect.width + bar.SpaceBetweenColonistsHorizontal - 2f, alpha);
    }
}

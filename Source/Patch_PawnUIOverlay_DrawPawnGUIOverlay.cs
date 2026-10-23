using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Draws title lines under colonists' names on the map.</summary>
[HarmonyPatch(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay))]
public static class Patch_PawnUIOverlay_DrawPawnGUIOverlay
{
    public static void Postfix(Pawn ___pawn)
    {
        Pawn pawn = ___pawn;
        if (pawn == null || !pawn.Spawned || !pawn.IsColonist || !pawn.RaceProps.Humanlike)
        {
            return;
        }

        // Same conditions under which vanilla skips the name label.
        if (pawn.Map.fogGrid.IsFogged(pawn.Position) || WorldComponent_GravshipController.CutsceneInProgress || (pawn.IsMutant && pawn.mutant.Def.hideLabel))
        {
            return;
        }

        var lines = TitleLabels.For(pawn, forMap: true, out float offset);
        if (lines.Count == 0)
        {
            return;
        }

        Vector2 pos = GenMapUI.LabelDrawPosFor(pawn, -0.6f);
        LabelDrawer.DrawLines(lines, pos.x, pos.y + LabelDrawer.NameHeight + offset, 9999f, 1f);
    }
}

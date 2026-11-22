using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Useful Marks can draw a one-line title label directly under a pawn's name; title lines start below it.</summary>
[StaticConstructorOnStartup]
public static class UsefulMarksCompat
{
    // Useful Marks in Colony Groups draws Useful Marks' label on Colony Groups' bar too.
    public static readonly bool LabelsOnColonyGroupsBar = ModsConfig.IsActive("TrueMogician.UsefulMarksInColonyGroups");

    private static Func<Pawn, bool, bool>? shouldDrawLabel;
    private static Func<bool>? drawOnColonistBar;

    static UsefulMarksCompat()
    {
        if (!ModsConfig.IsActive("andromeda.usefulmarks"))
        {
            return;
        }

        MethodInfo? method = AccessTools.Method("UsefulMarks.LabelUtils:GetShouldDrawMarkLabel", new[] { typeof(Pawn), typeof(bool) });
        FieldInfo? setting = AccessTools.Field(AccessTools.TypeByName("UsefulMarks.Settings"), "DrawTitlesOnColonistBar");
        if (method == null || setting == null || !setting.IsStatic)
        {
            Log.Warning("[Colonist Titles] Useful Marks found, but its labels have changed. Titles may overlap its labels.");
            return;
        }

        shouldDrawLabel = AccessTools.MethodDelegate<Func<Pawn, bool, bool>>(method);
        drawOnColonistBar = Expression.Lambda<Func<bool>>(Expression.Field(null, setting)).Compile();
    }

    /// <summary>Pushes what Useful Marks stacks under a map name (bottom marks, Ammo Readout's ammo block) below the title lines.</summary>
    public static void PatchIndentBlock(Harmony harmony)
    {
        if (shouldDrawLabel == null)
        {
            return;
        }

        MethodInfo? target = AccessTools.Method("UsefulMarks.NamePlatePatches:DrawIndentBlock");
        if (target == null)
        {
            Log.Warning("[Colonist Titles] Useful Marks' indent block not found. Its marks and Ammo Readout's ammo may overlap titles.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(typeof(UsefulMarksCompat), nameof(DrawIndentBlockPrefix)) { priority = Priority.First });
    }

    // Matches Useful Marks' DrawIndentBlock(Pawn pawn, Rect bgRect, ref float offsetMarks, bool InWorld).
    private static void DrawIndentBlockPrefix(Pawn pawn, ref float offsetMarks, bool InWorld)
    {
        if (!InWorld || pawn == null || !pawn.Spawned || !pawn.IsColonist || !pawn.RaceProps.Humanlike)
        {
            return;
        }

        var lines = TitleLabels.For(pawn, forMap: true, out _);
        if (lines.Count > 0)
        {
            offsetMarks += Mathf.Max(0f, lines.Count * LabelDrawer.ScaledLineHeight + ColonistTitlesMod.Settings.MapOffset);
        }
    }

    /// <summary>Height of Useful Marks' label under this pawn's name, or 0 when it draws none.</summary>
    public static float LabelHeight(Pawn pawn, bool onColonistBar)
    {
        if (shouldDrawLabel == null)
        {
            return 0f;
        }

        try
        {
            if (onColonistBar && !drawOnColonistBar!())
            {
                return 0f;
            }

            return shouldDrawLabel(pawn, onColonistBar) ? Text.LineHeightOf(GameFont.Tiny) : 0f;
        }
        catch (Exception e)
        {
            Log.Error("[Colonist Titles] Reading Useful Marks' labels failed; titles may overlap them. " + e.Message);
            shouldDrawLabel = null;
            return 0f;
        }
    }
}

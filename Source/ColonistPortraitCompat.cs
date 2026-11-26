using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>[LFS] UI Rethemed - Colonist Portrait draws its own colonist card; title lines go under its name instead of vanilla's.</summary>
public static class ColonistPortraitCompat
{
    private const float Gap = 2f;

    private static Func<bool>? barOn;
    private static Func<bool>? handingOnName;
    private static Func<float, float>? belowCard;
    private static Func<float, float>? columnGap;

    // Name pill bottom of the card being drawn, recorded when the themed card hands its name to the game's pawn label.
    private static Pawn? pillPawn;
    private static float pillBottom;

    // Room under each row's names the themed layout was last built with (session); the most any card drawn needed in the frame being counted.
    private static float layoutRoom;
    private static float frameRoom;
    private static int roomFrame = -1;

    /// <summary>Reads the themed bar's layout and patches the game's pawn label to find its name pill.</summary>
    public static void Patch(Harmony harmony)
    {
        if (!ModsConfig.IsActive("LazyFridayStudio.UIRethemed.ColonistPortrait"))
        {
            return;
        }

        MethodInfo? on = AccessTools.PropertyGetter("UIRethemed.ColonistPortrait.PortraitPart:BarOn");
        MethodInfo? handing = AccessTools.PropertyGetter("UIRethemed.ColonistPortrait.PortraitCard:HandingOnName");
        MethodInfo? room = AccessTools.Method("UIRethemed.ColonistPortrait.PortraitLayout:WeaponRoom", new[] { typeof(float) });
        MethodInfo? gap = AccessTools.Method("UIRethemed.ColonistPortrait.BarLayout:Gap", new[] { typeof(float) });
        MethodInfo? rowStep = AccessTools.Method("UIRethemed.ColonistPortrait.BarLayout:RowStep", new[] { typeof(float) });
        MethodInfo? label = AccessTools.Method(typeof(GenMapUI), nameof(GenMapUI.DrawPawnLabel), new[] { typeof(Pawn), typeof(Rect), typeof(float), typeof(float), typeof(Dictionary<string, string>), typeof(GameFont), typeof(bool), typeof(bool) });
        if (on?.ReturnType != typeof(bool) || handing?.ReturnType != typeof(bool) || room?.ReturnType != typeof(float) || gap?.ReturnType != typeof(float) || rowStep?.ReturnType != typeof(float) || label == null)
        {
            Log.Warning("[Colonist Titles] UI Rethemed - Colonist Portrait found, but its colonist bar has changed. Titles may overlap its cards.");
            return;
        }

        try
        {
            handingOnName = AccessTools.MethodDelegate<Func<bool>>(handing);
            belowCard = AccessTools.MethodDelegate<Func<float, float>>(room);
            columnGap = AccessTools.MethodDelegate<Func<float, float>>(gap);
            harmony.Patch(label, postfix: new HarmonyMethod(typeof(ColonistPortraitCompat), nameof(DrawPawnLabelPostfix)));
            harmony.Patch(rowStep, postfix: new HarmonyMethod(typeof(ColonistPortraitCompat), nameof(RowStepPostfix)));
            barOn = AccessTools.MethodDelegate<Func<bool>>(on);
        }
        catch (Exception e)
        {
            Log.Warning("[Colonist Titles] UI Rethemed - Colonist Portrait found, but its colonist bar has changed. Titles may overlap its cards. " + e.Message);
        }
    }

    /// <summary>True while the themed colonist bar replaces vanilla's.</summary>
    public static bool BarOn
    {
        get
        {
            if (barOn == null)
            {
                return false;
            }

            try
            {
                return barOn();
            }
            catch (Exception e)
            {
                Log.Error("[Colonist Titles] Reading UI Rethemed - Colonist Portrait's bar failed; titles go back to vanilla's place. " + e.Message);
                barOn = null;
                return false;
            }
        }
    }

    /// <summary>Top of the first title line under a themed card: below its name, or below its squares and weapons when it shows no name.</summary>
    public static float LinesTop(Rect card, Pawn colonist)
    {
        float top = pillPawn == colonist ? pillBottom : card.yMax + belowCard!(card.height);
        pillPawn = null;
        return top + Gap;
    }

    /// <summary>Space between two cards on the themed bar.</summary>
    public static float CardGap(float scale)
    {
        return columnGap!(scale);
    }

    // The themed card skips the game's label drawing with a prefix; postfixes still run and get its name pill.
    /// <summary>Counts the room a card's lines need (repaint only); when the most needed in a frame differs from the layout's, the bar is rebuilt.</summary>
    public static void NoteLines(int count)
    {
        if (roomFrame != RealTime.frameCount)
        {
            if (roomFrame >= 0 && Mathf.Abs(frameRoom - layoutRoom) > 0.5f)
            {
                layoutRoom = frameRoom;
                Find.ColonistBar.MarkColonistsDirty();
            }

            roomFrame = RealTime.frameCount;
            frameRoom = 0f;
        }

        if (count > 0)
        {
            frameRoom = Mathf.Max(frameRoom, Gap + ColonistTitlesMod.Settings.BarOffset + count * LabelDrawer.ScaledLineHeight);
        }
    }

    // Matches BarLayout.RowStep(float scale): each row, and the frame around it, gets room for the lines under its names.
    private static void RowStepPostfix(ref float __result)
    {
        __result += layoutRoom;
    }

    // Matches GenMapUI.DrawPawnLabel(Pawn pawn, Rect bgRect, ...) in 1.6 and 1.7.
    private static void DrawPawnLabelPostfix(Pawn pawn, Rect bgRect)
    {
        if (handingOnName != null && handingOnName())
        {
            pillPawn = pawn;
            pillBottom = bgRect.yMax;
        }
    }
}

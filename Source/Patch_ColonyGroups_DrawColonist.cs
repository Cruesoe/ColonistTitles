using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Colony Groups replaces the colonist bar with its own drawer; draws title lines there too.</summary>
[HarmonyPatch]
public static class Patch_ColonyGroups_DrawColonist
{
    private static FieldInfo? barField;
    private static PropertyInfo? scaleProperty;
    private static PropertyInfo? spaceProperty;
    private static MethodInfo? alphaMethod;

    public static bool Prepare()
    {
        Type? drawer = DrawerType();
        Type? utils = AccessTools.TypeByName("TacticalGroups.TacticUtils");
        Type? bar = AccessTools.TypeByName("TacticalGroups.TacticalColonistBar");
        if (drawer == null || utils == null || bar == null)
        {
            return false;
        }

        barField = AccessTools.Field(utils, "TacticalColonistBar");
        scaleProperty = AccessTools.Property(bar, "Scale");
        spaceProperty = AccessTools.Property(bar, "SpaceBetweenColonistsHorizontal");
        alphaMethod = AccessTools.Method(bar, "GetEntryRectAlpha", new[] { typeof(Rect) });
        if (barField == null || scaleProperty == null || spaceProperty == null || alphaMethod == null)
        {
            Log.Warning("[Colonist Titles] Colony Groups found, but its colonist bar has changed. Titles will not show on its bar.");
            return false;
        }

        return true;
    }

    public static MethodBase? TargetMethod()
    {
        return AccessTools.Method(DrawerType(), "DrawColonist", new[] { typeof(Rect), typeof(Pawn), typeof(Map), typeof(bool), typeof(bool) });
    }

    public static void Postfix(Rect rect, Pawn colonist, Map pawnMap)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        object? bar = barField!.GetValue(null);
        if (bar == null)
        {
            return;
        }

        float scale = (float)scaleProperty!.GetValue(bar)!;
        float space = (float)spaceProperty!.GetValue(bar)!;
        float alpha = (float)alphaMethod!.Invoke(bar, new object[] { rect })!;
        Patch_ColonistBarColonistDrawer_DrawColonist.DrawBarLines(rect, colonist, pawnMap, scale, space, alpha);
    }

    private static Type? DrawerType()
    {
        return AccessTools.TypeByName("TacticalGroups.TacticalGroups_ColonistBarColonistDrawer");
    }
}

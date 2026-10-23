using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Colony Groups replaces the colonist bar with its own drawer; draws title lines there too.</summary>
[HarmonyPatch]
public static class Patch_ColonyGroups_DrawColonist
{
    private static Func<object?>? getBar;
    private static Func<object, float>? getScale;
    private static Func<object, float>? getSpace;
    private static Func<object, Rect, float>? getAlpha;

    public static bool Prepare()
    {
        Type? drawer = DrawerType();
        Type? utils = AccessTools.TypeByName("TacticalGroups.TacticUtils");
        Type? bar = AccessTools.TypeByName("TacticalGroups.TacticalColonistBar");
        if (drawer == null || utils == null || bar == null)
        {
            return false;
        }

        FieldInfo? barField = AccessTools.Field(utils, "TacticalColonistBar");
        PropertyInfo? scale = AccessTools.Property(bar, "Scale");
        PropertyInfo? space = AccessTools.Property(bar, "SpaceBetweenColonistsHorizontal");
        MethodInfo? alpha = AccessTools.Method(bar, "GetEntryRectAlpha", new[] { typeof(Rect) });
        if (barField == null || !barField.IsStatic || scale == null || space == null || alpha == null)
        {
            Log.Warning("[Colonist Titles] Colony Groups found, but its colonist bar has changed. Titles will not show on its bar.");
            return false;
        }

        // Typed accessors, compiled once, so drawing makes no reflection calls.
        ParameterExpression instance = Expression.Parameter(typeof(object));
        ParameterExpression rect = Expression.Parameter(typeof(Rect));
        Expression typed = Expression.Convert(instance, bar);
        getBar = Expression.Lambda<Func<object?>>(Expression.Convert(Expression.Field(null, barField), typeof(object))).Compile();
        getScale = Expression.Lambda<Func<object, float>>(Expression.Property(typed, scale), instance).Compile();
        getSpace = Expression.Lambda<Func<object, float>>(Expression.Property(typed, space), instance).Compile();
        getAlpha = Expression.Lambda<Func<object, Rect, float>>(Expression.Call(typed, alpha, rect), instance, rect).Compile();
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

        object? bar = getBar!();
        if (bar == null)
        {
            return;
        }

        Patch_ColonistBarColonistDrawer_DrawColonist.DrawBarLines(rect, colonist, pawnMap, getScale!(bar), getSpace!(bar), getAlpha!(bar, rect), nextRowTop: null, otherModLabels: false);
    }

    private static Type? DrawerType()
    {
        return AccessTools.TypeByName("TacticalGroups.TacticalGroups_ColonistBarColonistDrawer");
    }
}

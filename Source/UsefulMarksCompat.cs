using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ColonistTitles;

/// <summary>Useful Marks can draw a one-line title label directly under a pawn's name; title lines start below it.</summary>
[StaticConstructorOnStartup]
public static class UsefulMarksCompat
{
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

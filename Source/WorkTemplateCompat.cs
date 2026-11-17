using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ColonistTitles;

/// <summary>Reads the work templates [LFS] UI Rethemed - Work has assigned to a pawn.</summary>
[StaticConstructorOnStartup]
public static class WorkTemplateCompat
{
    public const string ModName = "[LFS] UI Rethemed - Work";

    public static readonly bool Active = ModsConfig.IsActive("LazyFridayStudio.UIRethemed.Work");

    // The pawn's template names as UI Rethemed - Work tags them ("Cook", or "Cook +1" for several); null when none.
    private static Func<Pawn, string?>? tagLabel;

    static WorkTemplateCompat()
    {
        if (!Active)
        {
            return;
        }

        Type? roles = AccessTools.TypeByName("UIRethemed.Work.Roles");
        MethodInfo? all = roles == null ? null : AccessTools.Method(roles, "All", new[] { typeof(Pawn) });
        MethodInfo? tag = roles == null ? null : AccessTools.Method(roles, "TagLabel");
        if (all == null || tag == null || tag.ReturnType != typeof(string))
        {
            Log.Warning("[Colonist Titles] UI Rethemed - Work found, but its templates have changed. Work templates won't show.");
            return;
        }

        try
        {
            ParameterExpression pawn = Expression.Parameter(typeof(Pawn), "pawn");
            tagLabel = Expression.Lambda<Func<Pawn, string?>>(Expression.Call(tag, Expression.Call(all, pawn)), pawn).Compile();
        }
        catch (Exception e)
        {
            Log.Warning("[Colonist Titles] UI Rethemed - Work found, but its templates have changed. Work templates won't show. " + e.Message);
        }
    }

    /// <summary>The names of the work templates assigned to this pawn, or null when it has none.</summary>
    public static string? Label(Pawn pawn)
    {
        if (tagLabel == null)
        {
            return null;
        }

        try
        {
            string? label = tagLabel(pawn);
            return label.NullOrEmpty() ? null : label;
        }
        catch (Exception e)
        {
            Log.Error("[Colonist Titles] Reading UI Rethemed - Work's templates failed; work templates won't show. " + e.Message);
            tagLabel = null;
            return null;
        }
    }
}

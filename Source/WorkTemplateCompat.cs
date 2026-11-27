using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ColonistTitles;

/// <summary>Sets colonists' titles to the work template [LFS] UI Rethemed - Work has assigned them.</summary>
public static class WorkTemplateCompat
{
    public const string ModName = "[LFS] UI Rethemed - Work";

    public static readonly bool Active = ModsConfig.IsActive("LazyFridayStudio.UIRethemed.Work");

    // The label of the pawn's first work template; null when it has none.
    private static Func<Pawn, string?>? firstLabel;

    // The label of a template object passed to UI Rethemed - Work's methods.
    private static Func<object, string?>? templateLabel;

    /// <summary>Patches UI Rethemed - Work's template assignment, edit, removal and game load.</summary>
    public static void Patch(Harmony harmony)
    {
        if (!Active)
        {
            return;
        }

        Type? roles = AccessTools.TypeByName("UIRethemed.Work.Roles");
        Type? templates = AccessTools.TypeByName("UIRethemed.Work.Templates");
        Type? workRoles = AccessTools.TypeByName("UIRethemed.Work.WorkRoles");
        Type? template = AccessTools.TypeByName("UIRethemed.Work.WorkTemplate");
        MethodInfo? of = roles == null ? null : AccessTools.Method(roles, "Of", new[] { typeof(Pawn) });
        PropertyInfo? labelCap = template == null ? null : AccessTools.Property(template, "LabelCap");
        MethodInfo? setRoles = workRoles == null ? null : AccessTools.Method(workRoles, "SetRoles", new[] { typeof(Pawn), typeof(IEnumerable<string>) });
        MethodInfo? loadedGame = workRoles == null ? null : AccessTools.Method(workRoles, "LoadedGame");
        MethodInfo? save = templates == null || template == null ? null : AccessTools.Method(templates, "Save", new[] { template });
        MethodInfo? remove = templates == null || template == null ? null : AccessTools.Method(templates, "Remove", new[] { template });
        if (of?.ReturnType != template || labelCap?.PropertyType != typeof(string) || setRoles == null || loadedGame == null || save == null || remove == null)
        {
            Log.Warning("[Colonist Titles] UI Rethemed - Work found, but its templates have changed. Work templates won't become titles.");
            return;
        }

        try
        {
            ParameterExpression pawn = Expression.Parameter(typeof(Pawn), "pawn");
            ParameterExpression first = Expression.Variable(template!, "first");
            firstLabel = Expression.Lambda<Func<Pawn, string?>>(
                Expression.Block(new[] { first },
                    Expression.Assign(first, Expression.Call(of, pawn)),
                    Expression.Condition(Expression.Equal(first, Expression.Constant(null, template!)), Expression.Constant(null, typeof(string)), Expression.Property(first, labelCap))),
                pawn).Compile();

            ParameterExpression obj = Expression.Parameter(typeof(object), "obj");
            templateLabel = Expression.Lambda<Func<object, string?>>(Expression.Property(Expression.Convert(obj, template!), labelCap), obj).Compile();

            harmony.Patch(setRoles, prefix: new HarmonyMethod(typeof(WorkTemplateCompat), nameof(SetRolesPrefix)), postfix: new HarmonyMethod(typeof(WorkTemplateCompat), nameof(SetRolesPostfix)));
            harmony.Patch(remove, prefix: new HarmonyMethod(typeof(WorkTemplateCompat), nameof(RemovePrefix)), postfix: new HarmonyMethod(typeof(WorkTemplateCompat), nameof(RemovePostfix)));
            harmony.Patch(save, postfix: new HarmonyMethod(typeof(WorkTemplateCompat), nameof(SyncAll)));
            harmony.Patch(loadedGame, postfix: new HarmonyMethod(typeof(WorkTemplateCompat), nameof(SyncAll)));
        }
        catch (Exception e)
        {
            firstLabel = null;
            Log.Warning("[Colonist Titles] UI Rethemed - Work found, but its templates have changed. Work templates won't become titles. " + e.Message);
        }
    }

    private static bool On => firstLabel != null && ColonistTitlesMod.Settings.WorkTemplateAsTitle;

    private static string? FirstLabel(Pawn pawn)
    {
        if (firstLabel == null)
        {
            return null;
        }

        try
        {
            string? label = firstLabel(pawn);
            return label.NullOrEmpty() ? null : label;
        }
        catch (Exception e)
        {
            Log.Error("[Colonist Titles] Reading UI Rethemed - Work's templates failed; work templates won't become titles. " + e.Message);
            firstLabel = null;
            return null;
        }
    }

    /// <summary>Sets the pawn's title to its first work template; with none, clears a title that was the template it just lost.</summary>
    private static void Sync(Pawn pawn, string? previous)
    {
        Pawn_StoryTracker? story = pawn?.story;
        if (story == null)
        {
            return;
        }

        string? label = FirstLabel(pawn!);
        if (label != null)
        {
            if (story.title != label)
            {
                story.title = label;
                ColonistTitlesSettings.Version++;
            }
        }
        else if (previous != null && story.title == previous)
        {
            story.title = null;
            ColonistTitlesSettings.Version++;
        }
    }

    /// <summary>Gives every free colonist its work template as title.</summary>
    public static void SyncAll()
    {
        if (!On || Current.Game == null)
        {
            return;
        }

        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.ToArray())
        {
            Sync(pawn, null);
        }
    }

    private static void SetRolesPrefix(Pawn pawn, out string? __state)
    {
        __state = On && pawn != null ? FirstLabel(pawn) : null;
    }

    private static void SetRolesPostfix(Pawn pawn, string? __state)
    {
        if (On && pawn != null)
        {
            Sync(pawn, __state);
        }
    }

    private static void RemovePrefix(object[] __args, out string? __state)
    {
        __state = null;
        if (On && templateLabel != null && __args.Length > 0 && __args[0] != null)
        {
            try
            {
                __state = templateLabel(__args[0]);
            }
            catch (Exception)
            {
                __state = null;
            }
        }
    }

    private static void RemovePostfix(string? __state)
    {
        if (!On || Current.Game == null)
        {
            return;
        }

        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.ToArray())
        {
            Sync(pawn, __state);
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistTitles;

public readonly struct TitleLine
{
    public readonly string Text;
    public readonly Color Color;

    public TitleLine(string text, Color color)
    {
        Text = text;
        Color = color;
    }
}

/// <summary>Builds and caches the label lines drawn under a pawn's name.</summary>
public static class TitleLabels
{
    // Rebuild at most this often (in frames) so changes while paused still show.
    private const int RefreshFrames = 30;

    private class Entry
    {
        public readonly List<TitleLine> Bar = new List<TitleLine>();
        public readonly List<TitleLine> Map = new List<TitleLine>();
        public int Frame = int.MinValue;
        public int Version = -1;
    }

    private static readonly Dictionary<Pawn, Entry> cache = new Dictionary<Pawn, Entry>();

    public static List<TitleLine> For(Pawn pawn, bool forMap)
    {
        if (!cache.TryGetValue(pawn, out Entry? entry))
        {
            if (cache.Count > 500)
            {
                cache.Clear();
            }

            entry = new Entry();
            cache[pawn] = entry;
        }

        if (entry.Version != ColonistTitlesSettings.Version || RealTime.frameCount - entry.Frame >= RefreshFrames)
        {
            Rebuild(pawn, entry);
        }

        return forMap ? entry.Map : entry.Bar;
    }

    private static void Rebuild(Pawn pawn, Entry entry)
    {
        ColonistTitlesSettings settings = ColonistTitlesMod.Settings;
        entry.Frame = RealTime.frameCount;
        entry.Version = ColonistTitlesSettings.Version;
        entry.Bar.Clear();
        entry.Map.Clear();

        string? title = Title(pawn, settings);
        if (title != null)
        {
            TitleLine line = new TitleLine(title, settings.TitleColor);
            if (settings.BarTitle)
            {
                entry.Bar.Add(line);
            }

            if (settings.MapTitle)
            {
                entry.Map.Add(line);
            }
        }

        string? royal = RoyalTitle(pawn);
        if (royal != null)
        {
            TitleLine line = new TitleLine(royal, settings.RoyalTitleColor);
            if (settings.BarRoyalTitle)
            {
                entry.Bar.Add(line);
            }

            if (settings.MapRoyalTitle)
            {
                entry.Map.Add(line);
            }
        }

        Precept_Role? role = IdeoRole(pawn);
        if (role != null)
        {
            Color color = settings.UseIdeoColorForRole && role.ideo != null ? role.ideo.Color : settings.IdeoRoleColor;
            TitleLine line = new TitleLine(role.LabelForPawn(pawn).CapitalizeFirst(), color);
            if (settings.BarIdeoRole)
            {
                entry.Bar.Add(line);
            }

            if (settings.MapIdeoRole)
            {
                entry.Map.Add(line);
            }
        }
    }

    private static string? Title(Pawn pawn, ColonistTitlesSettings settings)
    {
        Pawn_StoryTracker? story = pawn.story;
        if (story == null)
        {
            return null;
        }

        if (!story.title.NullOrEmpty())
        {
            return story.title.CapitalizeFirst();
        }

        if (!settings.UseBackstoryTitle)
        {
            return null;
        }

        string backstory = story.TitleShortCap;
        return backstory.NullOrEmpty() ? null : backstory;
    }

    private static string? RoyalTitle(Pawn pawn)
    {
        if (!ModsConfig.RoyaltyActive)
        {
            return null;
        }

        RoyalTitle? title = pawn.royalty?.MostSeniorTitle;
        return title?.def.GetLabelCapFor(pawn);
    }

    private static Precept_Role? IdeoRole(Pawn pawn)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return null;
        }

        return pawn.Ideo?.GetRole(pawn);
    }
}

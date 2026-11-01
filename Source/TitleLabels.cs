using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Builds and caches the label lines drawn under a pawn's name.</summary>
public static class TitleLabels
{
    // Rebuild at most this often (in frames) so changes while paused still show.
    private const int RefreshFrames = 30;

    private class Entry
    {
        public readonly List<TitleLine> Bar = new List<TitleLine>();
        public readonly List<TitleLine> Map = new List<TitleLine>();
        public float BarOffset;
        public float MapOffset;
        public int Frame = int.MinValue;
        public int Version = -1;
    }

    private static readonly Dictionary<Pawn, Entry> cache = new Dictionary<Pawn, Entry>();
    private static Game? cachedGame;

    /// <summary>The lines for a pawn, and how far below the name they start (past other mods' labels).</summary>
    public static List<TitleLine> For(Pawn pawn, bool forMap, out float offset)
    {
        // Drop pawns from a previous game so it isn't kept in memory.
        if (Current.Game != cachedGame)
        {
            cache.Clear();
            cachedGame = Current.Game;
        }

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

        offset = forMap ? entry.MapOffset : entry.BarOffset;
        return forMap ? entry.Map : entry.Bar;
    }

    private static void Rebuild(Pawn pawn, Entry entry)
    {
        ColonistTitlesSettings settings = ColonistTitlesMod.Settings;
        entry.Frame = RealTime.frameCount;
        entry.Version = ColonistTitlesSettings.Version;
        entry.Bar.Clear();
        entry.Map.Clear();
        entry.BarOffset = UsefulMarksCompat.LabelHeight(pawn, onColonistBar: true);
        entry.MapOffset = UsefulMarksCompat.LabelHeight(pawn, onColonistBar: false);

        string? title = Title(pawn, settings);
        if (title != null)
        {
            AddLine(entry, title, settings.TitleColor, settings.BarTitle, settings.MapTitle);
        }

        string? royal = RoyalTitle(pawn);
        if (royal != null)
        {
            AddLine(entry, royal, settings.RoyalTitleColor, settings.BarRoyalTitle, settings.MapRoyalTitle);
        }

        Precept_Role? role = IdeoRole(pawn);
        if (role != null)
        {
            Color color = settings.UseIdeoColorForRole && role.ideo != null ? role.ideo.Color : settings.IdeoRoleColor;
            AddLine(entry, role.LabelForPawn(pawn).CapitalizeFirst(), color, settings.BarIdeoRole, settings.MapIdeoRole);
        }
    }

    // The bar and map draw at different widths, so each list has its own line and measurement cache.
    private static void AddLine(Entry entry, string text, Color color, bool bar, bool map)
    {
        if (bar)
        {
            entry.Bar.Add(new TitleLine(text, color));
        }

        if (map)
        {
            entry.Map.Add(new TitleLine(text, color));
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

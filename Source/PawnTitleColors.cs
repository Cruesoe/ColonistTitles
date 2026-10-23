using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Per-pawn title colors, saved with the game.</summary>
public class PawnTitleColors : GameComponent
{
    private Dictionary<Pawn, Color> colors = new Dictionary<Pawn, Color>();
    private List<Pawn>? keys;
    private List<Color>? values;

    public PawnTitleColors(Game game)
    {
    }

    public static PawnTitleColors? Instance => Current.Game?.GetComponent<PawnTitleColors>();

    public static Color For(Pawn pawn)
    {
        if (Instance != null && Instance.colors.TryGetValue(pawn, out Color color))
        {
            return color;
        }

        return ColonistTitlesMod.Settings.TitleColor;
    }

    public static bool HasCustom(Pawn pawn)
    {
        return Instance?.colors.ContainsKey(pawn) ?? false;
    }

    public static void Set(Pawn pawn, Color color)
    {
        PawnTitleColors? instance = Instance;
        if (instance == null)
        {
            return;
        }

        color.a = 1f;
        if (color.IndistinguishableFrom(ColonistTitlesMod.Settings.TitleColor))
        {
            instance.colors.Remove(pawn);
        }
        else
        {
            instance.colors[pawn] = color;
        }

        ColonistTitlesSettings.Version++;
    }

    public static void Clear(Pawn pawn)
    {
        if (Instance?.colors.Remove(pawn) == true)
        {
            ColonistTitlesSettings.Version++;
        }
    }

    public static void ClearAll()
    {
        Instance?.colors.Clear();
        ColonistTitlesSettings.Version++;
    }

    public override void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            colors.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        }

        Scribe_Collections.Look(ref colors, "colors", LookMode.Reference, LookMode.Value, ref keys, ref values);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            colors ??= new Dictionary<Pawn, Color>();
            colors.RemoveAll(pair => pair.Key == null);
        }
    }
}

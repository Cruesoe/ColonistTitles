using UnityEngine;
using Verse;

namespace ColonistTitles;

public class ColonistTitlesSettings : ModSettings
{
    public static readonly Color DefaultTitleColor = new Color(1f, 0.8f, 0.4f);
    public static readonly Color DefaultRoyalTitleColor = new Color(1f, 2f / 3f, 0f);
    public static readonly Color DefaultIdeoRoleColor = new Color(1f, 2f / 3f, 0f);

    public bool BarTitle = true;
    public bool BarRoyalTitle = true;
    public bool BarIdeoRole = true;
    public bool MapTitle = true;
    public bool MapRoyalTitle;
    public bool MapIdeoRole;
    public bool UseBackstoryTitle = true;
    public bool DrawBackground = true;
    public bool UseIdeoColorForRole = true;
    public Color TitleColor = DefaultTitleColor;
    public Color RoyalTitleColor = DefaultRoyalTitleColor;
    public Color IdeoRoleColor = DefaultIdeoRoleColor;
    public float Opacity = 0.9f;

    // Bumped on any change so cached labels rebuild.
    public static int Version;

    public void Reset()
    {
        BarTitle = true;
        BarRoyalTitle = true;
        BarIdeoRole = true;
        MapTitle = true;
        MapRoyalTitle = false;
        MapIdeoRole = false;
        UseBackstoryTitle = true;
        DrawBackground = true;
        UseIdeoColorForRole = true;
        TitleColor = DefaultTitleColor;
        RoyalTitleColor = DefaultRoyalTitleColor;
        IdeoRoleColor = DefaultIdeoRoleColor;
        Opacity = 0.9f;
        Version++;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref BarTitle, "barTitle", true);
        Scribe_Values.Look(ref BarRoyalTitle, "barRoyalTitle", true);
        Scribe_Values.Look(ref BarIdeoRole, "barIdeoRole", true);
        Scribe_Values.Look(ref MapTitle, "mapTitle", true);
        Scribe_Values.Look(ref MapRoyalTitle, "mapRoyalTitle");
        Scribe_Values.Look(ref MapIdeoRole, "mapIdeoRole");
        Scribe_Values.Look(ref UseBackstoryTitle, "useBackstoryTitle", true);
        Scribe_Values.Look(ref DrawBackground, "drawBackground", true);
        Scribe_Values.Look(ref UseIdeoColorForRole, "useIdeoColorForRole", true);
        Scribe_Values.Look(ref TitleColor, "titleColor", DefaultTitleColor);
        Scribe_Values.Look(ref RoyalTitleColor, "royalTitleColor", DefaultRoyalTitleColor);
        Scribe_Values.Look(ref IdeoRoleColor, "ideoRoleColor", DefaultIdeoRoleColor);
        Scribe_Values.Look(ref Opacity, "opacity", 0.9f);
        Version++;
    }
}

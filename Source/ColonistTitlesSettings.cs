using UnityEngine;
using Verse;

namespace ColonistTitles;

public class ColonistTitlesSettings : ModSettings
{
    public static readonly Color DefaultTitleColor = new Color(1f, 0.8f, 0.4f);
    public static readonly Color DefaultRoyalTitleColor = new Color(1f, 2f / 3f, 0f);
    public static readonly Color DefaultIdeoRoleColor = new Color(1f, 2f / 3f, 0f);
    public static readonly Color DefaultWorkTemplateColor = new Color(0.65f, 0.85f, 0.55f);

    public bool BarTitle = true;
    public bool BarWorkTemplate = true;
    public bool BarRoyalTitle = true;
    public bool BarIdeoRole = true;
    public bool MapTitle;
    public bool MapWorkTemplate;
    public bool MapRoyalTitle;
    public bool MapIdeoRole;
    public bool UseBackstoryTitle = true;
    public bool DrawBackground = true;
    public bool UseIdeoColorForRole = true;
    public Color TitleColor = DefaultTitleColor;
    public Color RoyalTitleColor = DefaultRoyalTitleColor;
    public Color IdeoRoleColor = DefaultIdeoRoleColor;
    public Color WorkTemplateColor = DefaultWorkTemplateColor;
    public float Opacity = 0.9f;
    public float TextScale = 1f;

    // Vertical shift of the title lines in pixels; positive moves them down.
    public float BarOffset;
    public float MapOffset;

    // Bumped on any change so cached labels rebuild.
    public static int Version;

    public void Reset()
    {
        BarTitle = true;
        BarWorkTemplate = true;
        BarRoyalTitle = true;
        BarIdeoRole = true;
        MapTitle = false;
        MapWorkTemplate = false;
        MapRoyalTitle = false;
        MapIdeoRole = false;
        UseBackstoryTitle = true;
        DrawBackground = true;
        UseIdeoColorForRole = true;
        TitleColor = DefaultTitleColor;
        RoyalTitleColor = DefaultRoyalTitleColor;
        IdeoRoleColor = DefaultIdeoRoleColor;
        WorkTemplateColor = DefaultWorkTemplateColor;
        Opacity = 0.9f;
        TextScale = 1f;
        BarOffset = 0f;
        MapOffset = 0f;
        Version++;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref BarTitle, "barTitle", true);
        Scribe_Values.Look(ref BarWorkTemplate, "barWorkTemplate", true);
        Scribe_Values.Look(ref BarRoyalTitle, "barRoyalTitle", true);
        Scribe_Values.Look(ref BarIdeoRole, "barIdeoRole", true);
        Scribe_Values.Look(ref MapTitle, "mapTitle");
        Scribe_Values.Look(ref MapWorkTemplate, "mapWorkTemplate");
        Scribe_Values.Look(ref MapRoyalTitle, "mapRoyalTitle");
        Scribe_Values.Look(ref MapIdeoRole, "mapIdeoRole");
        Scribe_Values.Look(ref UseBackstoryTitle, "useBackstoryTitle", true);
        Scribe_Values.Look(ref DrawBackground, "drawBackground", true);
        Scribe_Values.Look(ref UseIdeoColorForRole, "useIdeoColorForRole", true);
        Scribe_Values.Look(ref TitleColor, "titleColor", DefaultTitleColor);
        Scribe_Values.Look(ref RoyalTitleColor, "royalTitleColor", DefaultRoyalTitleColor);
        Scribe_Values.Look(ref IdeoRoleColor, "ideoRoleColor", DefaultIdeoRoleColor);
        Scribe_Values.Look(ref WorkTemplateColor, "workTemplateColor", DefaultWorkTemplateColor);
        Scribe_Values.Look(ref Opacity, "opacity", 0.9f);
        Scribe_Values.Look(ref TextScale, "textScale", 1f);
        Scribe_Values.Look(ref BarOffset, "barOffset");
        Scribe_Values.Look(ref MapOffset, "mapOffset");
        Version++;
    }
}

using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Mod settings page: display toggles and colors with a live preview.</summary>
public static class SettingsWindow
{
    private const float RowHeight = 30f;
    private const float Gap = 12f;
    private const float SwatchSize = 24f;
    private const float ResetWidth = 90f;
    private const float ToggleWidth = 100f;
    private const float MinOffset = -20f;
    private const float MaxOffset = 40f;

    public static void Draw(Rect inRect, ColonistTitlesSettings settings)
    {
        Text.Font = GameFont.Small;
        bool royalty = ModsConfig.RoyaltyActive;
        bool ideology = ModsConfig.IdeologyActive;
        string royaltyTip = "ColonistTitles_RequiresDlc".Translate("Royalty");
        string ideologyTip = "ColonistTitles_RequiresDlc".Translate("Ideology");
        bool workTemplates = WorkTemplateCompat.Active;
        string workTemplateTip = workTemplates ? "ColonistTitles_ShowWorkTemplate_Desc".Translate() : "ColonistTitles_RequiresMod".Translate(WorkTemplateCompat.ModName);

        Rect bottom = new Rect(inRect.x, inRect.yMax - RowHeight, inRect.width, RowHeight);
        if (Widgets.ButtonText(new Rect(bottom.xMax - 200f, bottom.y, 200f, RowHeight), "ColonistTitles_ResetAll".Translate()))
        {
            settings.Reset();
        }

        float columnWidth = (inRect.width - Gap * 2f) / 2f;
        Rect left = new Rect(inRect.x, inRect.y, columnWidth, 0f);
        Rect right = new Rect(left.xMax + Gap * 2f, inRect.y, columnWidth, 0f);

        bool changed = false;
        float y = left.y;
        Header(left.x, ref y, columnWidth, "ColonistTitles_SectionLines".Translate());
        LineHeadings(left.x, ref y, columnWidth);
        changed |= LineRow(left.x, ref y, columnWidth, "ColonistTitles_ShowTitle".Translate(), ref settings.BarTitle, ref settings.MapTitle);
        changed |= LineRow(left.x, ref y, columnWidth, "ColonistTitles_ShowWorkTemplate".Translate(), ref settings.BarWorkTemplate, ref settings.MapWorkTemplate, !workTemplates, workTemplateTip);
        changed |= LineRow(left.x, ref y, columnWidth, "ColonistTitles_ShowRoyalTitle".Translate(), ref settings.BarRoyalTitle, ref settings.MapRoyalTitle, !royalty, royaltyTip);
        changed |= LineRow(left.x, ref y, columnWidth, "ColonistTitles_ShowIdeoRole".Translate(), ref settings.BarIdeoRole, ref settings.MapIdeoRole, !ideology, ideologyTip);
        changed |= Slider(left.x, ref y, columnWidth, "ColonistTitles_BarOffset".Translate(settings.BarOffset.ToString("+0;-0;0")), ref settings.BarOffset, MinOffset, MaxOffset, 1f);
        changed |= Slider(left.x, ref y, columnWidth, "ColonistTitles_MapOffset".Translate(settings.MapOffset.ToString("+0;-0;0")), ref settings.MapOffset, MinOffset, MaxOffset, 1f);
        y += Gap;
        Header(left.x, ref y, columnWidth, "ColonistTitles_SectionDisplay".Translate());
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_UseBackstoryTitle".Translate(), ref settings.UseBackstoryTitle, tooltip: "ColonistTitles_UseBackstoryTitle_Desc".Translate());
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_DrawBackground".Translate(), ref settings.DrawBackground);
        changed |= Slider(left.x, ref y, columnWidth, "ColonistTitles_Opacity".Translate(settings.Opacity.ToStringPercent()), ref settings.Opacity, 0.25f, 1f, 0.05f);
        changed |= Slider(left.x, ref y, columnWidth, "ColonistTitles_TextSize".Translate(settings.TextScale.ToStringPercent()), ref settings.TextScale, 0.5f, 2f, 0.05f);

        y = right.y;
        Header(right.x, ref y, columnWidth, "ColonistTitles_SectionColors".Translate());
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_TitleColor".Translate(), settings.TitleColor, ColonistTitlesSettings.DefaultTitleColor, c => settings.TitleColor = c);
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_WorkTemplateColor".Translate(), settings.WorkTemplateColor, ColonistTitlesSettings.DefaultWorkTemplateColor, c => settings.WorkTemplateColor = c, !workTemplates, workTemplateTip);
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_RoyalTitleColor".Translate(), settings.RoyalTitleColor, ColonistTitlesSettings.DefaultRoyalTitleColor, c => settings.RoyalTitleColor = c, !royalty, royaltyTip);
        changed |= Checkbox(right.x, ref y, columnWidth, "ColonistTitles_UseIdeoColorForRole".Translate(), ref settings.UseIdeoColorForRole, !ideology, ideologyTip);
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_IdeoRoleColor".Translate(), settings.IdeoRoleColor, ColonistTitlesSettings.DefaultIdeoRoleColor, c => settings.IdeoRoleColor = c, !ideology || settings.UseIdeoColorForRole, ideology ? "ColonistTitles_IdeoRoleColorDisabled".Translate().ToString() : ideologyTip);
        y += Gap;
        Header(right.x, ref y, columnWidth, "ColonistTitles_SectionPreview".Translate());
        y = Preview(new Rect(right.x, y, columnWidth, 0f), settings, workTemplates, royalty, ideology);

        if (changed)
        {
            ColonistTitlesSettings.Version++;
        }
    }

    private static void Header(float x, ref float y, float width, string label)
    {
        Rect rect = new Rect(x, y, width, RowHeight);
        GUI.color = Widgets.SeparatorLabelColor;
        using (new TextBlock(TextAnchor.LowerLeft))
        {
            Widgets.Label(rect, label);
        }

        GUI.color = Widgets.SeparatorLineColor;
        Widgets.DrawLineHorizontal(x, rect.yMax, width);
        GUI.color = Color.white;
        y += RowHeight + 6f;
    }

    // A label row with a slider under it; returns true when the value changed.
    private static bool Slider(float x, ref float y, float width, string label, ref float value, float min, float max, float roundTo)
    {
        Widgets.Label(new Rect(x, y, width, RowHeight), label);
        y += 24f;
        float result = Widgets.HorizontalSlider(new Rect(x, y, width, RowHeight), value, min, max, roundTo: roundTo);
        y += RowHeight;
        if (result == value)
        {
            return false;
        }

        value = result;
        return true;
    }

    // Column headings over the line rows' Colonist bar and Map checkboxes.
    private static void LineHeadings(float x, ref float y, float width)
    {
        using (new TextBlock(TextAnchor.MiddleCenter))
        {
            GUI.color = Widgets.SeparatorLabelColor;
            Widgets.Label(new Rect(x + width - ToggleWidth * 2f, y, ToggleWidth, RowHeight), "ColonistTitles_SectionBar".Translate());
            Widgets.Label(new Rect(x + width - ToggleWidth, y, ToggleWidth, RowHeight), "ColonistTitles_SectionMap".Translate());
            GUI.color = Color.white;
        }

        y += RowHeight;
    }

    // A line's label with its Colonist bar and Map checkboxes; returns true when either changed.
    private static bool LineRow(float x, ref float y, float width, string label, ref bool bar, ref bool map, bool disabled = false, string? tooltip = null)
    {
        Rect rect = new Rect(x, y, width, RowHeight);
        y += RowHeight;
        bool barBefore = bar;
        bool mapBefore = map;
        Widgets.DrawHighlightIfMouseover(rect);
        if (!tooltip.NullOrEmpty())
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        using (new TextBlock(TextAnchor.MiddleLeft))
        {
            Widgets.Label(new Rect(rect.x + 2f, rect.y, rect.width - ToggleWidth * 2f - 4f, RowHeight), label);
        }

        float boxY = rect.y + (RowHeight - 24f) / 2f;
        Widgets.Checkbox(rect.xMax - ToggleWidth * 1.5f - 12f, boxY, ref bar, disabled: disabled);
        Widgets.Checkbox(rect.xMax - ToggleWidth * 0.5f - 12f, boxY, ref map, disabled: disabled);
        return bar != barBefore || map != mapBefore;
    }

    private static bool Checkbox(float x, ref float y, float width, string label, ref bool value, bool disabled = false, string? tooltip = null)
    {
        Rect rect = new Rect(x, y, width, RowHeight);
        bool before = value;
        Widgets.DrawHighlightIfMouseover(rect);
        if (!tooltip.NullOrEmpty())
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        Widgets.CheckboxLabeled(rect.ContractedBy(2f, 3f), label, ref value, disabled);
        y += RowHeight;
        return value != before;
    }

    private static void ColorRow(float x, ref float y, float width, string label, Color current, Color defaultColor, Action<Color> set, bool disabled = false, string? tooltip = null)
    {
        Rect rect = new Rect(x, y, width, RowHeight);
        y += RowHeight;
        if (!tooltip.NullOrEmpty() && disabled)
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        Rect swatch = new Rect(rect.x + 2f, rect.y + (RowHeight - SwatchSize) / 2f, SwatchSize, SwatchSize);
        Rect reset = new Rect(rect.xMax - ResetWidth, rect.y + 2f, ResetWidth, RowHeight - 4f);
        Rect labelRect = new Rect(swatch.xMax + 8f, rect.y, reset.x - swatch.xMax - 12f, RowHeight);

        GUI.color = disabled ? Widgets.InactiveColor : Color.white;
        Widgets.DrawBoxSolidWithOutline(swatch, disabled ? current * 0.5f : current, Color.white);
        using (new TextBlock(TextAnchor.MiddleLeft))
        {
            Widgets.Label(labelRect, label);
        }

        GUI.color = Color.white;
        if (disabled)
        {
            return;
        }

        Rect clickable = new Rect(swatch.x, rect.y, labelRect.xMax - swatch.x, RowHeight);
        Widgets.DrawHighlightIfMouseover(clickable);
        TooltipHandler.TipRegion(clickable, "ColonistTitles_ClickToChoose".Translate());
        if (Widgets.ButtonInvisible(clickable))
        {
            Find.WindowStack.Add(new Dialog_TitleColorPicker(current, defaultColor, set));
        }

        if (Widgets.ButtonText(reset, "ColonistTitles_Reset".Translate(), active: !current.IndistinguishableFrom(defaultColor)))
        {
            set(defaultColor);
            ColonistTitlesSettings.Version++;
        }
    }

    // Draws a sample colonist name with the lines the current settings would show; returns the bottom y.
    private static float Preview(Rect rect, ColonistTitlesSettings settings, bool workTemplates, bool royalty, bool ideology)
    {
        List<TitleLine> lines = new List<TitleLine>();
        if (settings.BarTitle || settings.MapTitle)
        {
            lines.Add(new TitleLine("ColonistTitles_PreviewTitle".Translate(), settings.TitleColor));
        }

        if (workTemplates && (settings.BarWorkTemplate || settings.MapWorkTemplate))
        {
            lines.Add(new TitleLine("ColonistTitles_PreviewWorkTemplate".Translate(), settings.WorkTemplateColor));
        }

        if (royalty && (settings.BarRoyalTitle || settings.MapRoyalTitle))
        {
            lines.Add(new TitleLine("ColonistTitles_PreviewRoyalTitle".Translate(), settings.RoyalTitleColor));
        }

        if (ideology && (settings.BarIdeoRole || settings.MapIdeoRole))
        {
            Color roleColor = settings.UseIdeoColorForRole ? PreviewIdeoColor() : settings.IdeoRoleColor;
            lines.Add(new TitleLine("ColonistTitles_PreviewIdeoRole".Translate(), roleColor));
        }

        float height = 16f + LabelDrawer.LineHeight + Math.Max(LabelDrawer.LineHeight, lines.Count * LabelDrawer.ScaledLineHeight) + 16f;
        rect.height = height;
        Widgets.DrawBoxSolidWithOutline(rect, new Color(0.12f, 0.13f, 0.12f), new Color(1f, 1f, 1f, 0.2f));

        float y = rect.y + 16f;
        string name = "ColonistTitles_PreviewName".Translate();
        using (new TextBlock(GameFont.Small, TextAnchor.UpperCenter))
        {
            float width = Text.CalcSize(name).x;
            Rect background = new Rect(rect.center.x - width / 2f - 6f, y, width + 12f, LabelDrawer.LineHeight);
            GUI.DrawTexture(background, TexUI.GrayTextBG);
            GUI.color = GenMapUI.DefaultThingLabelColor;
            Widgets.Label(new Rect(rect.center.x - width / 2f, y - 2f, width, 100f), name);
            GUI.color = Color.white;
        }

        if (lines.Count == 0)
        {
            using (new TextBlock(TextAnchor.UpperCenter))
            {
                GUI.color = Widgets.InactiveColor;
                Widgets.Label(new Rect(rect.x, y + LabelDrawer.LineHeight, rect.width, LabelDrawer.LineHeight + 6f), "ColonistTitles_PreviewNothing".Translate());
                GUI.color = Color.white;
            }
        }
        else
        {
            LabelDrawer.DrawLines(lines, rect.center.x, y + LabelDrawer.LineHeight, rect.width - 20f, 1f);
        }

        return rect.yMax;
    }

    // The player's primary ideoligion color in a game; a sample color on the main menu.
    private static Color PreviewIdeoColor()
    {
        Ideo? ideo = Current.ProgramState == ProgramState.Playing ? Faction.OfPlayer?.ideos?.PrimaryIdeo : null;
        return ideo?.Color ?? new Color(0.55f, 0.75f, 0.45f);
    }
}

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

    public static void Draw(Rect inRect, ColonistTitlesSettings settings)
    {
        Text.Font = GameFont.Small;
        bool royalty = ModsConfig.RoyaltyActive;
        bool ideology = ModsConfig.IdeologyActive;
        string royaltyTip = "ColonistTitles_RequiresDlc".Translate("Royalty");
        string ideologyTip = "ColonistTitles_RequiresDlc".Translate("Ideology");

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
        Header(left.x, ref y, columnWidth, "ColonistTitles_SectionBar".Translate());
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_ShowTitle".Translate(), ref settings.BarTitle);
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_ShowRoyalTitle".Translate(), ref settings.BarRoyalTitle, !royalty, royaltyTip);
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_ShowIdeoRole".Translate(), ref settings.BarIdeoRole, !ideology, ideologyTip);
        y += Gap;
        Header(left.x, ref y, columnWidth, "ColonistTitles_SectionMap".Translate());
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_ShowTitle".Translate(), ref settings.MapTitle);
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_ShowRoyalTitle".Translate(), ref settings.MapRoyalTitle, !royalty, royaltyTip);
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_ShowIdeoRole".Translate(), ref settings.MapIdeoRole, !ideology, ideologyTip);
        y += Gap;
        Header(left.x, ref y, columnWidth, "ColonistTitles_SectionDisplay".Translate());
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_UseBackstoryTitle".Translate(), ref settings.UseBackstoryTitle, tooltip: "ColonistTitles_UseBackstoryTitle_Desc".Translate());
        changed |= Checkbox(left.x, ref y, columnWidth, "ColonistTitles_DrawBackground".Translate(), ref settings.DrawBackground);
        Rect sliderLabel = new Rect(left.x, y, columnWidth, RowHeight);
        Widgets.Label(sliderLabel, "ColonistTitles_Opacity".Translate(settings.Opacity.ToStringPercent()));
        y += 24f;
        float opacity = Widgets.HorizontalSlider(new Rect(left.x, y, columnWidth, RowHeight), settings.Opacity, 0.25f, 1f, roundTo: 0.05f);
        if (opacity != settings.Opacity)
        {
            settings.Opacity = opacity;
            changed = true;
        }

        y = right.y;
        Header(right.x, ref y, columnWidth, "ColonistTitles_SectionColors".Translate());
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_TitleColor".Translate(), settings.TitleColor, ColonistTitlesSettings.DefaultTitleColor, c => settings.TitleColor = c);
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_RoyalTitleColor".Translate(), settings.RoyalTitleColor, ColonistTitlesSettings.DefaultRoyalTitleColor, c => settings.RoyalTitleColor = c, !royalty, royaltyTip);
        changed |= Checkbox(right.x, ref y, columnWidth, "ColonistTitles_UseIdeoColorForRole".Translate(), ref settings.UseIdeoColorForRole, !ideology, ideologyTip);
        ColorRow(right.x, ref y, columnWidth, "ColonistTitles_IdeoRoleColor".Translate(), settings.IdeoRoleColor, ColonistTitlesSettings.DefaultIdeoRoleColor, c => settings.IdeoRoleColor = c, !ideology || settings.UseIdeoColorForRole, ideology ? "ColonistTitles_IdeoRoleColorDisabled".Translate().ToString() : ideologyTip);
        y += Gap;
        Header(right.x, ref y, columnWidth, "ColonistTitles_SectionPreview".Translate());
        y = Preview(new Rect(right.x, y, columnWidth, 0f), settings, royalty, ideology);

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
    private static float Preview(Rect rect, ColonistTitlesSettings settings, bool royalty, bool ideology)
    {
        List<TitleLine> lines = new List<TitleLine>();
        if (settings.BarTitle || settings.MapTitle)
        {
            lines.Add(new TitleLine("ColonistTitles_PreviewTitle".Translate(), settings.TitleColor));
        }

        if (royalty && (settings.BarRoyalTitle || settings.MapRoyalTitle))
        {
            lines.Add(new TitleLine("ColonistTitles_PreviewRoyalTitle".Translate(), settings.RoyalTitleColor));
        }

        if (ideology && (settings.BarIdeoRole || settings.MapIdeoRole))
        {
            Color roleColor = settings.UseIdeoColorForRole ? new Color(0.55f, 0.75f, 0.45f) : settings.IdeoRoleColor;
            lines.Add(new TitleLine("ColonistTitles_PreviewIdeoRole".Translate(), roleColor));
        }

        float height = 16f + LabelDrawer.LineHeight + Math.Max(1, lines.Count) * LabelDrawer.LineHeight + 16f;
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
}

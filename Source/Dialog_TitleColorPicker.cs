using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Vanilla color picker (wheel, RGB/HSV/hex fields, palette) with a title-color palette.</summary>
public class Dialog_TitleColorPicker : Dialog_ColorPickerBase
{
    private static List<Color>? palette;

    private readonly Color defaultColor;
    private readonly Action<Color> onSave;

    public Dialog_TitleColorPicker(Color current, Color defaultColor, Action<Color> onSave)
        : base(Widgets.ColorComponents.All, Widgets.ColorComponents.All)
    {
        current.a = 1f;
        color = current;
        oldColor = current;
        this.defaultColor = defaultColor;
        this.onSave = onSave;
    }

    protected override bool ShowDarklight => false;

    protected override Color DefaultColor => defaultColor;

    protected override List<Color> PickableColors => palette ??= BuildPalette();

    protected override float ForcedColorValue => -1f;

    protected override bool ShowColorTemperatureBar => false;

    protected override void SaveColor(Color newColor)
    {
        newColor.a = 1f;
        onSave(newColor);
        ColonistTitlesSettings.Version++;
    }

    // 18 hues at three saturations (two 9-column rows each), then a row of greys.
    private static List<Color> BuildPalette()
    {
        List<Color> colors = new List<Color>();
        foreach (float saturation in new[] { 1f, 0.6f, 0.3f })
        {
            for (int hue = 0; hue < 18; hue++)
            {
                colors.Add(Color.HSVToRGB(hue / 18f, saturation, 1f));
            }
        }

        for (int step = 0; step < 9; step++)
        {
            float value = 1f - step * 0.1f;
            colors.Add(new Color(value, value, value));
        }

        return colors;
    }
}

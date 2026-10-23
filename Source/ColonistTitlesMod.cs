using UnityEngine;
using Verse;

namespace ColonistTitles;

public class ColonistTitlesMod : Mod
{
    public static ColonistTitlesSettings Settings = null!;

    public ColonistTitlesMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<ColonistTitlesSettings>();
    }

    public override string SettingsCategory()
    {
        return "ColonistTitles_ModName".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        SettingsWindow.Draw(inRect, Settings);
    }
}

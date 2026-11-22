using HarmonyLib;
using Verse;

namespace ColonistTitles;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        var harmony = new Harmony("cruesoe.colonisttitles");
        harmony.PatchAll(typeof(Startup).Assembly);
        UsefulMarksCompat.PatchIndentBlock(harmony);
    }
}

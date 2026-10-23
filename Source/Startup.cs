using HarmonyLib;
using Verse;

namespace ColonistTitles;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        new Harmony("cruesoe.colonisttitles").PatchAll();
    }
}

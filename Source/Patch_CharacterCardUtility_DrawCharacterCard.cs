using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonistTitles;

/// <summary>Adds a title-color button to the Bio tab, left of the rename button.</summary>
[HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard))]
[StaticConstructorOnStartup]
public static class Patch_CharacterCardUtility_DrawCharacterCard
{
    private const float ButtonSize = 30f;
    private const float ButtonSpacing = 40f;

    private static readonly Texture2D ButtonTex = ContentFinder<Texture2D>.Get("UI/ColonistTitles/TitleColor");

    // After the rename button, vanilla moves its button cursor left with "x -= 40f". Insert the button there.
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo namePawnDialog = AccessTools.Method(typeof(PawnNamingUtility), nameof(PawnNamingUtility.NamePawnDialog));
        MethodInfo doButton = AccessTools.Method(typeof(Patch_CharacterCardUtility_DrawCharacterCard), nameof(DoButton));
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        bool foundRename = false;
        bool patched = false;

        for (int i = 0; i < codes.Count; i++)
        {
            yield return codes[i];
            if (patched)
            {
                continue;
            }

            if (codes[i].Calls(namePawnDialog))
            {
                foundRename = true;
                continue;
            }

            if (foundRename && i >= 3 && codes[i].IsStloc() && codes[i - 1].opcode == OpCodes.Sub
                && codes[i - 2].opcode == OpCodes.Ldc_R4 && codes[i - 2].operand is float step && step == ButtonSpacing
                && codes[i - 3].IsLdloc())
            {
                yield return CodeInstruction.LoadLocal(codes[i].LocalIndex(), useAddress: true);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, doButton);
                patched = true;
            }
        }

        if (!patched)
        {
            Log.Error("[Colonist Titles] Could not find the Bio tab rename button. The title color button will not show.");
        }
    }

    public static void DoButton(ref float x, Pawn pawn)
    {
        if (pawn?.story == null || PawnTitleColors.Instance == null)
        {
            return;
        }

        Rect rect = new Rect(x, 0f, ButtonSize, ButtonSize);
        TooltipHandler.TipRegion(rect, "ColonistTitles_TitleColorButton".Translate(pawn.Named("PAWN")));
        Color color = PawnTitleColors.For(pawn);
        if (Widgets.ButtonImage(rect, ButtonTex, color))
        {
            Find.WindowStack.Add(new Dialog_TitleColorPicker(color, ColonistTitlesMod.Settings.TitleColor, newColor => PawnTitleColors.Set(pawn, newColor)));
        }

        x -= ButtonSpacing;
    }
}

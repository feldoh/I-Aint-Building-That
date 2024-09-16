using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace IAintBuildingThat.HarmonyPatches;

[HarmonyPatch(typeof(Ability), "Initialize")]
class AbilityPatch
{
	static void Postfix(Ability __instance)
	{
		var comp = new CompAbilityHide(__instance);
		if (__instance.comps == null) __instance.comps = [comp];
		else __instance.comps.Add(comp);
	}
}

[HarmonyPatch(typeof(Game), "LoadGame")]
class AbilityClear
{
	static void Prefix() => IAintBuildingThat.settings.AllAbilityHideComponents.Clear();
}

[HarmonyPatch(typeof(Gizmo), "RightClickFloatMenuOptions", MethodType.Getter)]
class AbilityRightClickPatch()
{
	static void Postfix(Gizmo __instance, ref IEnumerable<FloatMenuOption> __result)
	{
		if (__instance is not Command_Ability ab || ab.Ability.CompOfType<CompAbilityHide>() is not {} compAbilityHide) return;

		string menuText = compAbilityHide.hidden
			? "Taggerung_IAintBuildingThat_RestoreText"
			: "Taggerung_IAintBuildingThat_HideButtonText";
		
		__result = __result.AddItem(new FloatMenuOption(menuText.TranslateSimple(),
				() => compAbilityHide.hidden = !compAbilityHide.hidden));
	}
}

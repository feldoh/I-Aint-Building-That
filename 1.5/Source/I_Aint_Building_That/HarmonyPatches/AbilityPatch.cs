using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
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

[HarmonyPatch(typeof(Gizmo), "RightClickFloatMenuOptions", MethodType.Getter)]
class AbilityRightClickPatch()
{
	static void Postfix(Gizmo __instance, ref IEnumerable<FloatMenuOption> __result)
	{
		if (__instance is Command_Ability ab) 
			__result = __result.AddItem(new FloatMenuOption("Hide", () => IAintBuildingThat.settings.HiddenAbilities.Add(ab.Ability)));
	}
}
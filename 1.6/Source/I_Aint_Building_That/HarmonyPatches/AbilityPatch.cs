using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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

[HarmonyPatch(typeof(Command_Ability), nameof(Command_Ability.ProcessInput))]
class ProcessInputPatch
{
	static bool Prefix(Command_Ability __instance, Event ev)
	{
		if (ev is not { button: 1 } || !Input.GetKey(KeyCode.LeftAlt) || __instance.Ability.CompOfType<CompAbilityHide>() is not { } compAbilityHide) return true;

		string menuText = compAbilityHide.hidden
			? "Taggerung_IAintBuildingThat_RestoreText"
			: "Taggerung_IAintBuildingThat_HideButtonText";

		Find.WindowStack.Add(new FloatMenu([
			new FloatMenuOption(menuText.TranslateSimple(),
				() => compAbilityHide.hidden = !compAbilityHide.hidden)
		]));
		return false;
	}
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

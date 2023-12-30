using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace IAintBuildingThat.HarmonyPatches;

[HarmonyPatch(typeof(Window), "Close")]
static class PatchAutoCloseDubs
{
	public static Lazy<Type> dubsWindow = new(() => AccessTools.TypeByName("DubsMintMenus.MainTabWindow_MintArchitect"));

	[HarmonyPrefix]
	static bool Postfix(Window __instance)
	{
		return dubsWindow.Value == null || !PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking || !dubsWindow.Value.IsInstanceOfType(__instance);
	}
}

[HarmonyPatch(typeof(Designator), nameof(Designator.RightClickFloatMenuOptions), MethodType.Getter)]
static class PatchInHideMenuOptionToDesignator
{
	[HarmonyPostfix]
	static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Designator __instance)
	{
		foreach (FloatMenuOption opt in __result)
		{
			yield return opt;
		}

		switch (__instance)
		{
			case Designator_Place { PlacingDef: not null } dp:
				yield return new FloatMenuOption("Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple() + ": " + dp.LabelCap,
					() => IAintBuildingThat.settings.HideBuildable(dp.PlacingDef));
				break;
			case Designator_Dropdown dd:
			{
				HashSet<string> alreadyGone = [..IAintBuildingThat.settings.HiddenBuildables];
				foreach (Designator ddElement in dd.Elements)
				{
					if (ddElement is not Designator_Place { PlacingDef: not null } ddBuildable || !alreadyGone.Add(ddBuildable.PlacingDef.defName)) continue;
					yield return new FloatMenuOption("Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple() + ": " + ddBuildable.LabelCap,
						() => IAintBuildingThat.settings.HideBuildable(ddBuildable.PlacingDef));
				}

				break;
			}
		}
	}
}

[HarmonyPatch(typeof(Designator), nameof(Designator.ProcessInput))]
static class PatchInHideMenuOptionToDesignatorProcessInput
{
	public static bool IsRightClicking;

	[HarmonyPrefix]
	static bool Prefix(Designator __instance, Event ev)
	{
		IsRightClicking = false;
		if (ev.button != 1 || __instance is not Designator_Place { PlacingDef: not null } dp) return true;
		if (Find.WindowStack.IsOpen<FloatMenu>()) return true;
		IsRightClicking = true;
		return true;
	}

	[HarmonyPostfix]
	static void Postfix(Designator __instance, Event ev)
	{
		if (ev.button != 1 || __instance is not Designator_Place { PlacingDef: not null } dp || Find.WindowStack.IsOpen<FloatMenu>()) return;
		Find.WindowStack.Add(new FloatMenu([
			new FloatMenuOption($"{"Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple()}: {dp.LabelCap}",
				() => IAintBuildingThat.settings.HideBuildable(dp.PlacingDef))
		]));
	}
}

[HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.ProcessInput))]
static class PatchInHideMenuOptionToBuildDesignatorProcessInput
{
	public static BuildableDef CurrentPlacingDef;

	[HarmonyPrefix]
	static bool Prefix(Designator_Build __instance, Event ev)
	{
		CurrentPlacingDef = null;
		if (ev.button != 1 || __instance is not { PlacingDef: not null } || !__instance.PlacingDef.MadeFromStuff) return true;
		PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking = true;
		CurrentPlacingDef = __instance.PlacingDef;
		return true;
	}
}

[HarmonyPatch(typeof(FloatMenu), MethodType.Constructor, [typeof(List<FloatMenuOption>)])]
static class PatchInHideMenuOptionToFloatConstructor
{
	[HarmonyPrefix]
	static bool Prefix(ref List<FloatMenuOption> options)
	{
		if (!PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking || PatchInHideMenuOptionToBuildDesignatorProcessInput.CurrentPlacingDef is not { } dp) return true;
		options.Add(new FloatMenuOption($"{"Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple()}: {dp.LabelCap}",
			() => IAintBuildingThat.settings.HideBuildable(dp)));
		return true;
	}
}

[HarmonyPatch(typeof(ArchitectCategoryTab), "DoInfoBox")]
static class TrackDesignator
{
	[CanBeNull] public static Designator LatestDesignator;

	[HarmonyPostfix]
	static void Postfix(Designator designator)
	{
		if (designator != null) LatestDesignator = designator;
	}
}

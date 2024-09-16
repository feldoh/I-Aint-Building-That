using System;
using System.Collections.Generic;
using System.Linq;
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

				if (IAintBuildingThat.settings.HiddenBuildables.Contains(dp.PlacingDef.defName))
				{
					yield return new FloatMenuOption("Taggerung_IAintBuildingThat_RestoreText".TranslateSimple() + ": " + dp.LabelCap,
						() => IAintBuildingThat.settings.RestoreBuildable(dp.PlacingDef));
				}
				else
				{
					yield return new FloatMenuOption("Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple() + ": " + dp.LabelCap,
						() => IAintBuildingThat.settings.HideBuildable(dp.PlacingDef));
				}
				break;
			case Designator_Dropdown dd:
			{
				HashSet<string> alreadyGone = [..IAintBuildingThat.settings.HiddenBuildables];
				FloatMenuOption hideAll = null;
				foreach (Designator ddElement in dd.Elements)
				{
					if (ddElement is not Designator_Place { PlacingDef: not null } ddBuildable) continue;
					if (!IAintBuildingThat.DropdownGroupDefs.TryGetValue(ddBuildable.PlacingDef.designatorDropdown, out var groupDefs))
					{
						IAintBuildingThat.DropdownGroupDefs.SetOrAdd(ddBuildable.PlacingDef.designatorDropdown, [ddBuildable.PlacingDef]);
					}
					else
					{
						groupDefs.Add(ddBuildable.PlacingDef);
					}

					if (!alreadyGone.Add(ddBuildable.PlacingDef.defName))
					{
						yield return new FloatMenuOption("Taggerung_IAintBuildingThat_RestoreText".TranslateSimple() + ": " + ddBuildable.LabelCap,
							() => IAintBuildingThat.settings.RestoreBuildable(ddBuildable.PlacingDef));
						continue;
					}
					yield return new FloatMenuOption("Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple() + ": " + ddBuildable.LabelCap,
						() => IAintBuildingThat.settings.HideBuildable(ddBuildable.PlacingDef));
					hideAll ??= new FloatMenuOption("Taggerung_IAintBuildingThat_HideAllButtonText".TranslateSimple(),
						() => IAintBuildingThat.DropdownGroupDefs[ddBuildable.PlacingDef.designatorDropdown].Do(IAintBuildingThat.settings.HideBuildable));
				}
				if (hideAll != null) yield return hideAll;
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
		if (ev is not { button: 1 } || __instance is not Designator_Place { PlacingDef: not null } dp) return true;
		if (Find.WindowStack.IsOpen<FloatMenu>()) return true;
		IsRightClicking = true;
		return true;
	}

	[HarmonyPostfix]
	static void Postfix(Designator __instance, Event ev)
	{
		if (ev is not { button: 1 } || __instance is not Designator_Place { PlacingDef: not null } dp || Find.WindowStack.IsOpen<FloatMenu>()) return;
		List<FloatMenuOption> floatMenuOptions = [];
		if (dp.PlacingDef.designatorDropdown is {} dd)
		{
			if (!IAintBuildingThat.DropdownGroupDefs.TryGetValue(dd, out var groupDefs))
			{
				IAintBuildingThat.DropdownGroupDefs.SetOrAdd(dd, [dp.PlacingDef]);
			}
			else
			{
				groupDefs.Add(dp.PlacingDef);
			}

			if (!IAintBuildingThat.DropdownGroupFirstDef.TryGetValue(dd, out var firstDesignatorInGroup) || firstDesignatorInGroup == dp.PlacingDef)
			{
				if (firstDesignatorInGroup == null) IAintBuildingThat.DropdownGroupFirstDef.SetOrAdd(dd, dp.PlacingDef);
				floatMenuOptions.Add(new FloatMenuOption("Taggerung_IAintBuildingThat_HideAllButtonText".TranslateSimple(),
					() => IAintBuildingThat.DropdownGroupDefs[dd].Do(IAintBuildingThat.settings.HideBuildable)));
			}
		}

		if (IAintBuildingThat.settings.HiddenBuildables.Contains(dp.PlacingDef.defName))
		{
			floatMenuOptions.Add(new FloatMenuOption("Taggerung_IAintBuildingThat_RestoreText".TranslateSimple() + ": " + dp.LabelCap,
				() => IAintBuildingThat.settings.RestoreBuildable(dp.PlacingDef)));
		}
		else
		{
			floatMenuOptions.Add(new FloatMenuOption($"{"Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple()}: {dp.LabelCap}",
				() => IAintBuildingThat.settings.HideBuildable(dp.PlacingDef)));
		}
		Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
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
		if (ev is not { button: 1 } || __instance is not { PlacingDef: not null } || !__instance.PlacingDef.MadeFromStuff) return true;
		PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking = true;
		CurrentPlacingDef = __instance.PlacingDef;
		return true;
	}
}

[HarmonyPatch(typeof(FloatMenu), MethodType.Constructor, typeof(List<FloatMenuOption>))]
static class PatchInHideMenuOptionToFloatConstructor
{
	[HarmonyPrefix]
	static bool Prefix(ref List<FloatMenuOption> options)
	{
		if (!PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking || PatchInHideMenuOptionToBuildDesignatorProcessInput.CurrentPlacingDef is not { } dp) return true;
		
		if (IAintBuildingThat.settings.HiddenBuildables.Contains(dp.defName))
		{
			options.Add(new FloatMenuOption("Taggerung_IAintBuildingThat_RestoreText".TranslateSimple() + ": " + dp.LabelCap,
				() => IAintBuildingThat.settings.RestoreBuildable(dp)));
		}
		else
		{
			options.Add(new FloatMenuOption($"{"Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple()}: {dp.LabelCap}",
				() => IAintBuildingThat.settings.HideBuildable(dp)));
		}
		
		if (dp.designatorDropdown is {} dd)
		{
			if (!IAintBuildingThat.DropdownGroupDefs.TryGetValue(dd, out var groupDefs))
			{
				IAintBuildingThat.DropdownGroupDefs.SetOrAdd(dd, [dp]);
			}
			else
			{
				groupDefs.Add(dp);
			}

			if (!IAintBuildingThat.DropdownGroupFirstDef.TryGetValue(dd, out var firstDesignatorInGroup) || firstDesignatorInGroup == dp)
			{
				if (firstDesignatorInGroup == null) IAintBuildingThat.DropdownGroupFirstDef.SetOrAdd(dd, dp);
				options.Add(new FloatMenuOption("Taggerung_IAintBuildingThat_HideAllButtonText".TranslateSimple(),
					() => IAintBuildingThat.DropdownGroupDefs[dd].Do(IAintBuildingThat.settings.HideBuildable)));
			}
		}
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

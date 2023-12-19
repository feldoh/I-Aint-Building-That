using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace IAintBuildingThat.HarmonyPatches
{
	[HarmonyPatch(typeof(Gizmo), "RightClickFloatMenuOptions", MethodType.Getter)]
	static class PatchInHideMenuOption
	{
		[HarmonyPostfix]
		static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> opts)
		{
			foreach (FloatMenuOption opt in opts)
			{
				yield return opt;
			}

			if (!Find.WindowStack.IsOpen<MainTabWindow_Architect>()) yield break;
			Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator ?? TrackDesignator.LatestDesignator;
			if (selectedDesignator is not Designator_Place { PlacingDef: not null } dp) yield break;
			yield return new FloatMenuOption("Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple(),
				() => IAintBuildingThat.settings.HiddenBuildables.Add(dp.PlacingDef?.defName));
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
}

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
			if (Find.WindowStack.IsOpen<MainTabWindow_Architect>())
			{
				yield return new FloatMenuOption("Taggerung_IAintBuildingThat_HideButtonText".TranslateSimple(), () =>
				{
					Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator ?? TrackDesignator.LatestDesignator;
					if (selectedDesignator is not Designator_Place { PlacingDef: not null } dp) return;
					Log.Message($"Hiding Def {dp.PlacingDef?.defName}");
					IAintBuildingThat.settings.HiddenBuildables.Add(dp.PlacingDef?.defName);
				});
			}

			foreach (FloatMenuOption opt in opts)
			{
				yield return opt;
			}
		}
	}

	[HarmonyPatch(typeof(PlaceWorker), "IsBuildDesignatorVisible")]
	static class PatchOutHiddenBuildables
	{
		[HarmonyPostfix]
		static bool Postfix(bool result, BuildableDef def) => result && !IAintBuildingThat.settings.HiddenBuildables.Contains(def.defName);
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

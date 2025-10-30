using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace IAintBuildingThat.HarmonyPatches;

public static class PlaySettingsPatch
{
	[HarmonyPatch(typeof(PlaySettings))]
	public static class PlaySettings_BuildingHiding_Patch
	{
		public static readonly Texture2D ToggleTexOn = ContentFinder<Texture2D>.Get(
			"UI/IAintBuildingThat_Toggle_On"
		);

		public static readonly Texture2D ToggleTexOff = ContentFinder<Texture2D>.Get(
			"UI/IAintBuildingThat_Toggle_Off"
		);

		[HarmonyPatch(nameof(PlaySettings.DoPlaySettingsGlobalControls))]
		[HarmonyPostfix]
		public static void DoPlaySettingsGlobalControls_Patch(WidgetRow row)
		{
			Texture2D ToggleTex = IAintBuildingThat.settings.showHiddenButtons ? ToggleTexOff : ToggleTexOn;

			if (row.ButtonIcon(ToggleTex, "Taggerung_IAintBuildingThat_ShowHiddenButtonsLabel".Translate()))
			{
				if (Event.current.button == 0)
				{
					IAintBuildingThat.settings.showHiddenButtons = !IAintBuildingThat.settings.showHiddenButtons;
				}
			}
		}
	}
}

using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace IAintBuildingThat
{
	public class IAintBuildingThat : Mod
	{
		public static Settings settings;
		public static Dictionary<DesignatorDropdownGroupDef, BuildableDef> DropdownGroupFirstDef = new();
		public static Dictionary<DesignatorDropdownGroupDef, HashSet<BuildableDef>> DropdownGroupDefs = new();

		public IAintBuildingThat(ModContentPack content) : base(content)
		{
			// initialize settings
			settings = GetSettings<Settings>();

#if DEBUG
			Harmony.DEBUG = true;
#endif

			Harmony harmony = new Harmony("Taggerung.rimworld.I_Aint_Building_That.main");
			harmony.PatchAll();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "I Aint Building That";
		}
	}
}

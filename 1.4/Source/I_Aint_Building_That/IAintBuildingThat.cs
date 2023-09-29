using HarmonyLib;
using UnityEngine;
using Verse;

namespace IAintBuildingThat
{
	public class IAintBuildingThat : Mod
	{
		public static Settings settings;

		public IAintBuildingThat(ModContentPack content) : base(content)
		{
			Log.Message("Hello world from I Aint Building That");

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

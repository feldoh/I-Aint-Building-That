using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace IAintBuildingThat
{
	public class Settings : ModSettings
	{
		public HashSet<string> HiddenBuildables = new();

		public void DoWindowContents(Rect wrect)
		{
			Listing_Standard options = new();
			options.Begin(wrect);

			if (options.ButtonText("Restore All Hidden Defs"))
			{
				HiddenBuildables.Clear();
			}

			options.Gap();

			options.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref HiddenBuildables, "hiddenBuildables", LookMode.Value);
		}
	}
}

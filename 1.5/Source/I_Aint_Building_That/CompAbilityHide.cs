using RimWorld;
using RimWorld.Planet;
using Verse;

namespace IAintBuildingThat;

internal class CompAbilityHide : CompAbilityEffect
{
	private static CompProperties_AbilityEffect _default_cached = new();

	internal bool hidden = false;

	internal CompAbilityHide(Ability p)
	{
		parent = p;
		props = _default_cached;
		IAintBuildingThat.settings.AllAbilityHideComponents.Add(this);
	}

	public override bool ShouldHideGizmo => hidden;

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false) => true; // removes random nullreferenceexception +_+
	public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => true;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref hidden, "hidden", false);
	}
}

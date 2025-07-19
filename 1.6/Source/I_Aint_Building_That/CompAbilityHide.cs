using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace IAintBuildingThat;

internal class CompProperties_AbilityHide : CompProperties_AbilityEffect;

internal class CompAbilityHide : CompAbilityEffect
{
	private static CompProperties_AbilityHide _defaultCached = new();

	internal bool hidden = false;

	internal CompAbilityHide(Ability p)
	{
		parent = p;
		props = _defaultCached;
		IAintBuildingThat.settings.AllAbilityHideComponents.Add(this);
	}

	public override bool ShouldHideGizmo => hidden && !(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt));

	/**
	 * We have to override many of the methods because in the parent class they dig around in the props and assume things are set.
	 */
	public override bool Valid(LocalTargetInfo target, bool throwMessages = false) => true;
	public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => true;
	public override bool AICanTargetNow(LocalTargetInfo target) => true;
	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => true;
	public override bool CanApplyOn(GlobalTargetInfo target) => true;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref hidden, "hidden", false);
	}
}

using RimWorld;
using RimWorld.Planet;
using Verse;

namespace IAintBuildingThat;
internal class CompAbilityHide : CompAbilityEffect
{
	internal CompAbilityHide(Ability p) => parent = p;

  public override bool ShouldHideGizmo => IAintBuildingThat.settings.HiddenAbilities.Contains(parent);

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false) => true; // removes random nullreferenceexception +_+
	public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => true;
}

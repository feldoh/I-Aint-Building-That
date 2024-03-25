using Verse;

namespace IAintBuildingThat;

public class VisibilityPlaceWorker : PlaceWorker
{
	public override bool IsBuildDesignatorVisible(BuildableDef def) => !IAintBuildingThat.settings.HiddenBuildables.Contains(def.defName);
}

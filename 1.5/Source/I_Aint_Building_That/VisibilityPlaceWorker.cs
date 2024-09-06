using UnityEngine;
using Verse;

namespace IAintBuildingThat;

public class VisibilityPlaceWorker : PlaceWorker
{
	public override bool IsBuildDesignatorVisible(BuildableDef def)
	{
		return (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)) ||
		       !IAintBuildingThat.settings.HiddenBuildables.Contains(def.defName);
	}
}

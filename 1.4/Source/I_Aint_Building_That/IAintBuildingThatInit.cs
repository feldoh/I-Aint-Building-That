using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IAintBuildingThat;
using Verse;
using Log = IAintBuildingThat.Log;

[StaticConstructorOnStartup]
public static class IAintBuildingThatInit
{
	static IAintBuildingThatInit()
	{
		Type defDatabaseType = typeof(DefDatabase<>);

		GenDefDatabase.AllDefTypesWithDatabases()
			.Where(t => typeof(BuildableDef).IsAssignableFrom(t)).Do(c => Log.Message($"Making all {c}s hideable"));

		FieldInfo placeWorkersInstantiatedIntFieldInfo = AccessTools.Field(typeof(BuildableDef), "placeWorkersInstantiatedInt");

		GenDefDatabase.AllDefTypesWithDatabases()
			.Where(t => typeof(BuildableDef).IsAssignableFrom(t))
			.Select(t => defDatabaseType.MakeGenericType(t))
			.SelectMany(t => (t.GetProperty("AllDefs")?.GetValue(null) as IEnumerable<BuildableDef>)
				?.Where(b => !(b.placeWorkers?.Contains(typeof(VisibilityPlaceWorker)) ?? false)) ?? new BuildableDef[] { })
			.Do(b =>
			{
				b.placeWorkers ??= [];
				b.placeWorkers?.Add(typeof(VisibilityPlaceWorker));
				List<PlaceWorker> placeWorkers = placeWorkersInstantiatedIntFieldInfo.GetValue(b) as List<PlaceWorker>;
				placeWorkers?.Add((PlaceWorker)Activator.CreateInstance(typeof(VisibilityPlaceWorker)));
			});
	}
}

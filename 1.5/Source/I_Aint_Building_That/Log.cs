using System;

namespace IAintBuildingThat;

public class Log
{
	public static string ModTag = "<color=#b48af5>[IAintBuildingThat]</color>";
	public static void Message (string msg)
	{
		Verse.Log.Message($"{ModTag} {msg ?? "<null>"}");
	}

	public static void Warn(string msg)
	{
		Verse.Log.Warning($"{ModTag} {msg ?? "<null>"}");
	}

	public static void Error(string msg, Exception e = null)
	{
		Verse.Log.Error($"{ModTag} {msg ?? "<null>"}");
		if (e != null)
			Verse.Log.Error(e.ToString());
	}

}

using HarmonyLib;
using Verse;
using System.Reflection;

namespace Setosa;

[StaticConstructorOnStartup]
public static class Main {
	static Main() {
		// Initialize Harmony
		var harmony = new Harmony(ThisAssembly.Project.PackageId);
		// Reads all [HarmonyPatch] attributes in your assembly and applies them
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		Log.Message($"[{ThisAssembly.Info.Title}]: Initialized");
    }
}
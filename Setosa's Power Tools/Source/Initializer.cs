using System.Reflection;
using HarmonyLib;
using Verse;

namespace Setosa;

[StaticConstructorOnStartup]
public static class Initializer {
	static Initializer() {
#if DEBUG
		Harmony.DEBUG = true;
#endif
		// Initialize Harmony
		var harmony = new Harmony(ThisAssembly.Project.PackageId);
		// Reads all [HarmonyPatch] attributes in your assembly and applies them
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		Settings.Default.Offsets.Apply(ThingStatOffsetCollection.ApplyMode.Overwrite);
		Logger.Message("Initialized");
    }
}

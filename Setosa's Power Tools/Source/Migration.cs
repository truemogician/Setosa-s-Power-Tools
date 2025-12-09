using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Verse;

namespace Setosa;

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Migration {
	private const string OldThingPrefix = "advancedTool_";

	private const string NewThingPrefix = "AdvancedTool_";

	[HarmonyPostfix]
	public static void Postfix(Type defType, string defName, ref string? __result) {
		if (__result is not null)
			return;
		if (defType == typeof(ThingDef) && defName.StartsWith(OldThingPrefix)) {
			__result = NewThingPrefix + defName[OldThingPrefix.Length..];
			Logger.Message($"Migrated {defName} to {__result}");
		}
	}
}

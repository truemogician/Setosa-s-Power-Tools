using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Setosa;

[StaticConstructorOnStartup]
public static class DefModifier {
	static DefModifier() {
		Apply();
	}

	public static void Apply() {
		var advancedTools = DefDatabase<ThingDef>.AllDefs
			.Where(d => d.weaponTags?.Contains("AdvancedTool") == true);
		var offsetsDict = Mod.Settings.Offsets;
        foreach (var def in advancedTools) {
			var offsets = offsetsDict.GetValueOrDefault(def.defName);
			if (offsets is null) {
				Logger.Warning($"Stat offset for {def} not found", true);
				continue;
			}
			def.equippedStatOffsets = offsets.Select(o => o.ToModifier()).ToList();
		}
	}
}
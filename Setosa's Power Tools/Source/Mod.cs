using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Setosa;

[StaticConstructorOnStartup]
public class Mod : Verse.Mod {
	static Mod() {
#if DEBUG
		Harmony.DEBUG = true;
#endif
		// Initialize Harmony
		var harmony = new Harmony(ThisAssembly.Project.PackageId);
		// Reads all [HarmonyPatch] attributes in your assembly and applies them
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		Logger.Message("Initialized");
    }

	public static Settings Settings { get; private set; } = null!;

	public Mod(ModContentPack content) : base(content) {
		Settings = GetSettings<Settings>();
	}

	public override string SettingsCategory() => ThisAssembly.Info.Title;

	public override void DoSettingsWindowContents(Rect inRect) {
		var list = new Listing_Standard();
		list.Begin(inRect);

		var rect = list.GetRect(30f); // Reserve space for the row
		Widgets.Label(rect.LeftHalf(), "Preset:");

		if (Widgets.ButtonText(rect.RightHalf(), Settings.Preset.ToString())) {
			var options = Enum.GetValues(typeof(StatsPreset))
				.OfType<StatsPreset>()
				.Select(
					preset => new FloatMenuOption(preset.ToString(), () => Settings.Preset = preset)
				)
				.ToList();
			Find.WindowStack.Add(new FloatMenu(options));
		}

		list.Gap();
		foreach (var (thingDef, offsets) in StatOffsets.NormalPreset) {
			var thing = DefDatabase<ThingDef>.GetNamed(thingDef, false);
			if (thing is null) {
				Logger.Warning($"Thing {thingDef} not found", true);
				continue;
			}
			list.Label($"{thing.LabelCap} Offsets:");
			for (int i = 0; i < offsets.Count; ++i) {
				var offset = offsets[i];
				var stat = offset.Stat;
				if (stat is null) {
					Logger.Warning($"Stat {offset.DefName} not found", true);
					continue;
				}
				list.Label($"{stat.LabelCap}: {offset.Value:P0}");
				if (Settings.Preset == StatsPreset.Custom) {
					var sliderRect = list.GetRect(22f);
					var value = Widgets.HorizontalSlider(sliderRect, offset.Value, 0f, 2f, roundTo: 0.1f);
					offsets[i] = new StatOffset(offset.DefName!, value);
				}
            }
		}

        list.End();
		base.DoSettingsWindowContents(inRect);
	}
}
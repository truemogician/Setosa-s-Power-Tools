using System;
using System.Collections.Generic;
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

	private readonly ScrollView _scroll = new();

	public Mod(ModContentPack content) : base(content) {
		Settings.Default = GetSettings<Settings>();
    }

	private const float RowHeight = 30;

	private static string FormatValue(float value) {
		var color = value switch {
			> 0f => "green",
			0    => "white",
			_    => "red"
		};
		return $"<color={color}>{value:+0.##%;-0.##%;0%}</color>";
	}

    public override string SettingsCategory() => ThisAssembly.Info.Title;

	public override void DoSettingsWindowContents(Rect inRect) {
		var list = new Listing_Standard();
		list.Begin(inRect);

		var rect = list.GetRect(RowHeight);
		Widgets.Label(rect.LeftHalf(), "Preset:");
		if (Widgets.ButtonText(rect.RightHalf(), Settings.Default.Preset.ToString())) {
			var options = Enum.GetValues(typeof(StatsPreset))
				.OfType<StatsPreset>()
				.Select(preset => new FloatMenuOption(preset.ToString(), () => Settings.Default.Preset = preset)
				)
				.ToList();
			Find.WindowStack.Add(new FloatMenu(options));
		}

		list.GapLine(RowHeight / 2);

		var groups = Settings.Default.Offsets.Select(t => (ThingStatOffset)t)
			.GroupBy(t => t.ThingDef)
			.ToDictionary(g => g.Key, g => g.ToList());
		float height = (groups.Count + (Settings.Default.Preset == StatsPreset.Custom ? Settings.Default.Offsets.Count : groups.Count)) * RowHeight;

		var rest = new Rect(inRect) { y = list.CurHeight, height = inRect.height - list.CurHeight };
        var scrollList = _scroll.Begin(rest, height);

		var customUpdates = new List<ThingStatOffset>();
		foreach (var (thingDef, offsets) in groups) {
			var thing = DefDatabase<ThingDef>.GetNamed(thingDef, false);
			if (thing is null) {
				Logger.Warning($"Thing {thingDef} not found", true);
				continue;
			}
			using (new TransientStyle { Anchor = TextAnchor.MiddleCenter })
				scrollList.Label($"<b>{thing.LabelCap}</b>");
			if (Settings.Default.Preset != StatsPreset.Custom) {
				var texts = new List<string>();
				foreach (var tuple in offsets) {
					if (tuple.Stat is { } stat)
						texts.Add($"{FormatValue(tuple.Value)} {stat.label}");
					else
						Logger.Warning($"Stat {tuple.StatDef} not found", true);
				}
				using (new TransientStyle { Anchor = TextAnchor.MiddleCenter })
                    Widgets.Label(scrollList.GetRect(RowHeight), string.Join(", ", texts));
			}
			else {
				foreach (var tuple in offsets) {
					var stat = tuple.Stat;
					if (stat is null) {
						Logger.Warning($"Stat {tuple.StatDef} not found", true);
						continue;
					}
					var cols = scrollList.GetRect(RowHeight).FlexBox(["250", "50", "1fr"]);
					Widgets.Label(cols[0], $"{stat.LabelCap}:");
					Widgets.Label(cols[1], FormatValue(tuple.Value));
					if (Settings.Default.Preset == StatsPreset.Custom) {
						float maximum = Presets.Normal[tuple.ThingDef, tuple.StatDef] * 4;
						float newValue = Widgets.HorizontalSlider(
							rect: cols[2], 
							value: tuple.Value, 
							min: Math.Min(0f, maximum), 
							max: Math.Max(0f, maximum), 
							roundTo: 0.1f
						);
						if (!Mathf.Approximately(tuple.Value, newValue))
							customUpdates.Add(tuple.WithNewValue(newValue));
					}
				}
			}
		}

		_scroll.End();
		list.End();

        if (customUpdates.Count > 0) {
			var newCustomOffsets = Settings.Default.CustomOffsets.Clone();
			foreach ((string thing, string stat, float value) in customUpdates)
				newCustomOffsets[thing, stat] = value;
			Settings.Default.CustomOffsets = newCustomOffsets;
		}
	}

	public override void WriteSettings() {
		base.WriteSettings();
		Settings.Default.Offsets.Apply(ThingStatOffsetCollection.ApplyMode.Overwrite);
	}
}
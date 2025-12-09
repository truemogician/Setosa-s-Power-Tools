using Verse;

namespace Setosa;

public class Settings : ModSettings {
	public static Settings Default { get; internal set; } = null!;

	private StatsPreset _preset = StatsPreset.Normal;

	private ThingStatOffsetCollection? _customOffsets;

    public StatsPreset Preset {
		get => _preset;
		internal set => _preset = value;
	}

	public ThingStatOffsetCollection CustomOffsets {
		get => _customOffsets ??= Presets.Normal.Clone();
		internal set => _customOffsets = value;
	}

	public ThingStatOffsetCollection Offsets => Preset switch {
		StatsPreset.Nerfed => Presets.Nerfed,
		StatsPreset.Buffed => Presets.Buffed,
		StatsPreset.Custom => CustomOffsets,
		_                  => Presets.Normal
	};

	public override void ExposeData() {
		base.ExposeData();
		Scribe_Values.Look(ref _preset, "preset");
		Scribe_Deep.Look(ref _customOffsets, "customOffsets");
    }
}
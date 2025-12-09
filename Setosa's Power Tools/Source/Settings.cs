using Verse;

namespace Setosa;

public class Settings : ModSettings {
    private StatsPreset _preset = StatsPreset.Normal;

	private StatOffsetCollection? _customOffsets;

    public StatsPreset Preset {
		get => _preset;
		internal set => _preset = value;
	}

	public StatOffsetCollection CustomOffsets {
		get => _customOffsets ??= StatOffsetCollection.NormalPreset.Clone();
		internal set => _customOffsets = value;
	}

	public StatOffsetCollection Offsets => Preset switch {
		StatsPreset.Nerfed => StatOffsetCollection.NerfedPreset,
		StatsPreset.Buffed => StatOffsetCollection.BuffedPreset,
		StatsPreset.Custom => CustomOffsets,
		_                  => StatOffsetCollection.NormalPreset
	};

	public override void ExposeData() {
		base.ExposeData();
		Scribe_Values.Look(ref _preset, "preset");
		Scribe_Deep.Look(ref _customOffsets, "customOffsets");
    }
}
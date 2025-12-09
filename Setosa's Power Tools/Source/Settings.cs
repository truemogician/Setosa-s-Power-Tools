using Verse;

namespace Setosa;

public class Settings : ModSettings {
    private StatsPreset _preset = StatsPreset.Normal;

	private ThingStatOffsetCollection? _customOffsets;

    public StatsPreset Preset {
		get => _preset;
		internal set => _preset = value;
	}

	public ThingStatOffsetCollection CustomOffsets {
		get => _customOffsets ??= ThingStatOffsetCollection.NormalPreset.Clone();
		internal set => _customOffsets = value;
	}

	public ThingStatOffsetCollection Offsets => Preset switch {
		StatsPreset.Nerfed => ThingStatOffsetCollection.NerfedPreset,
		StatsPreset.Buffed => ThingStatOffsetCollection.BuffedPreset,
		StatsPreset.Custom => CustomOffsets,
		_                  => ThingStatOffsetCollection.NormalPreset
	};

	public override void ExposeData() {
		base.ExposeData();
		Scribe_Values.Look(ref _preset, "preset");
		Scribe_Deep.Look(ref _customOffsets, "customOffsets");
    }
}
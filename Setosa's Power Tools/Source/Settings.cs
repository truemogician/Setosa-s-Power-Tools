using Verse;

namespace Setosa;

public class Settings : ModSettings {
    private StatsPreset _preset = StatsPreset.Normal;

	private StatOffsets _customOffsets = StatOffsets.NormalPreset.Clone();


    public StatsPreset Preset {
		get => _preset;
		internal set => _preset = value;
	}

	public StatOffsets CustomOffsets => _customOffsets;

	public StatOffsets Offsets => Preset switch {
		StatsPreset.Nerfed => StatOffsets.NerfedPreset,
		StatsPreset.Buffed => StatOffsets.BuffedPreset,
		StatsPreset.Custom => CustomOffsets,
		_ => StatOffsets.NormalPreset
    };

	public override void ExposeData() {
		base.ExposeData();
		Scribe_Values.Look(ref _preset, "preset");
		Scribe_Deep.Look(ref _customOffsets, "customOffsets");
    }
}
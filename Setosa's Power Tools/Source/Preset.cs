using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Setosa;

public enum StatsPreset : byte {
	Normal,

	Nerfed,

	Buffed,

	Custom
}

public readonly struct StatOffset(string defName, float value) {
	public StatOffset(StatModifier modifier) : this(modifier.stat.defName, modifier.value) { }

	public string? DefName { get; } = defName;

	public float Value { get; } = value;

	public StatDef? Stat => DefName is null ? null : DefDatabase<StatDef>.GetNamed(DefName, false);

    public StatModifier? ToModifier() => Stat is {} stat ? new StatModifier { stat = stat, value = Value } : null;
}

public class StatOffsets : Dictionary<string, List<StatOffset>>, IExposable {
	private class Tuple(string thingDef, string statDef, float value): IExposable {
		private string? _thingDef = thingDef;

		private string? _statDef = statDef;

		private float _value = value;

		internal static IEnumerable<Tuple> Generate(StatOffsets collection) {
			foreach (var (key, list) in collection) {
				foreach (var offset in list)
					yield return new Tuple(key, offset.DefName!, offset.Value);
            }
		}

		public void ExposeData() {
			Scribe_Values.Look(ref _thingDef, "thing");
			Scribe_Values.Look(ref _statDef, "stat");
			Scribe_Values.Look(ref _value, "value");
			if (_thingDef is null || _statDef is null)
				Logger.Warning("Missing thingDef or/and statDef");
        }

		internal void AppendTo(StatOffsets collection) {
			if (_thingDef is null || _statDef is null)
				return;
			if (!collection.TryGetValue(_thingDef, out var list))
				collection[_thingDef] = list = [];
			list.Add(new StatOffset(_statDef, _value));
        }
	}

	public StatOffsets() { }

	public StatOffsets(IEnumerable<(string, List<StatOffset>)> collection) 
		: base(collection.Select(t => new KeyValuePair<string, List<StatOffset>>(t.Item1, t.Item2))) { }

	public StatOffsets(IDictionary<string, List<StatOffset>> dictionary) : base(dictionary) { }

	public static readonly StatOffsets NormalPreset = new() {
		{
			"advancedTool_IndustrialChainsaw",
			[
				("PlantWorkSpeed", 0.8f),
				("ButcheryFleshSpeed", 0.4f),
				("MoveSpeed", -0.25f)
			]
		},
		{
			"advancedTool_PowerSaw",
			[
				("ConstructionSpeed", 0.8f),
				("MoveSpeed", -0.25f)
			]
		},
		{
			"advancedTool_PowerAuger",
			[
				("PlantWorkSpeed", 1.2f),
				("MoveSpeed", -0.25f)
			]
		},
		{
			"advancedTool_Chisel",
			[
				("GeneralLaborSpeed", 0.4f),
				("SmoothingSpeed", 0.8f),
				("ConstructionSpeed", 0.2f)
			]
		},
		{
			"advancedTool_Jackhammer",
			[
				("MiningSpeed", 0.8f),
				("MoveSpeed", -0.25f)
			]
		},
		{
			"advancedTool_Drill",
			[
				("GeneralLaborSpeed", 0.8f),
				("ConstructionSpeed", 0.2f),
				("ButcheryMechanoidEfficiency", 0.8f),
				("ButcheryMechanoidSpeed", 0.8f)
			]
		},
		{
			"advancedTool_NailGun",
			[ ("ConstructionSpeed", 0.4f) ]
		}
	};

	public static readonly StatOffsets NerfedPreset = NormalPreset.Clone().Apply(v => v > 0 ? v / 2f : v);

	public static readonly StatOffsets BuffedPreset = NormalPreset.Clone().Apply(v => v > 0 ? v * 1.5f : v);

    public void Add(string thingDef, List<(string statDef, float value)> offsets) =>
		Add(thingDef, offsets.Select(t => new StatOffset(t.statDef, t.value)).ToList());

	public void ExposeData() {
		var tuples = Scribe.mode == LoadSaveMode.Saving ? Tuple.Generate(this).ToList() : [];
		Scribe_Collections.Look(ref tuples, "entries", LookMode.Deep); 
		if (Scribe.mode == LoadSaveMode.LoadingVars) {
			Clear();
			tuples.ForEach(t => t.AppendTo(this));
		}
    }

	public StatOffsets Clone() => new(this.Select(p => (p.Key, p.Value.ListFullCopy())));

	public StatOffsets Apply(Func<float, float> selector) => Apply(selector, (_, _) => true);
	public StatOffsets Apply(Func<float, float> selector, Func<string, string, bool> filter) {
		foreach (var (thingDef, offsets) in this) {
			for (int i = 0; i < offsets.Count; i++) {
				var offset = offsets[i];
				if (filter(offset.DefName!, thingDef))
					offsets[i] = new StatOffset(offset.DefName!, selector(offset.Value));
			}
		}
		return this;
	}
}
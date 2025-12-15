using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TrueMogician.Exceptions;
using TrueMogician.Extensions.Collections.Dictionary;
using TrueMogician.Extensions.Enumerable;
using Verse;

namespace Setosa;

using Base = TupleDictionary3D<string, string, float>;
using Tuple = (string ThingDef, string StatDef, float Value);

public enum StatsPreset : byte {
	Normal,

	Nerfed,

	Custom
}

public class ThingStatOffset : IExposable {
	private string? _thingDef;

	private string? _statDef;

	private float _value;

	public ThingStatOffset() { } // For reflection

	public ThingStatOffset(string thingDef, string statDef, float value) {
		_thingDef = thingDef;
		_statDef = statDef;
		_value = value;
	}

	public string ThingDef => _thingDef ?? throw new NotFoundException($"{nameof(ThingDef)} not found");

	public string StatDef => _statDef ?? throw new NotFoundException($"{nameof(StatDef)} not found");

	public float Value => _value;

	public ThingDef? Thing => DefDatabase<ThingDef>.GetNamed(ThingDef, false);

	public StatDef? Stat => DefDatabase<StatDef>.GetNamed(StatDef, false);

	internal bool Valid => _thingDef is not null && _statDef is not null;

	public void ExposeData() {
		string? valueStr = Scribe.mode == LoadSaveMode.Saving ? $"{_value:0.#}" : string.Empty;
		Scribe_Values.Look(ref _thingDef, "thing");
		Scribe_Values.Look(ref _statDef, "stat");
		Scribe_Values.Look(ref valueStr, "value");
		if (Scribe.mode == LoadSaveMode.LoadingVars) {
			if (valueStr is null || !float.TryParse(valueStr, out _value))
				_value = 0f;
		}
		if (_thingDef is null || _statDef is null)
			Logger.Warning("Missing thingDef or/and statDef");
	}

	public void Deconstruct(out string thingDef, out string statDef, out float value) {
		thingDef = ThingDef;
		statDef = StatDef;
		value = Value;
	}

	public ThingStatOffset WithNewValue(float value) => new(ThingDef, StatDef, value);

	public static implicit operator ThingStatOffset(Tuple tuple)
		=> new(tuple.ThingDef, tuple.StatDef, tuple.Value);

	public static implicit operator Tuple(ThingStatOffset offset)
		=> (offset.ThingDef, offset.StatDef, offset.Value);

	public static implicit operator StatModifier(ThingStatOffset offset)
		=> new() { stat = offset.Stat, value = offset.Value };
}

public class ThingStatOffsetCollection : Base, IExposable {
	public enum ApplyMode : byte {
		Set,
		Append,
		Overwrite
	}

	public ThingStatOffsetCollection() { }

	public ThingStatOffsetCollection(IEnumerable<Tuple> collection)
		=> collection.ForEach(Add);

	public ThingStatOffsetCollection(IDictionary3D<string, string, float> dictionary) : base(dictionary) { }

	public void Add(string thingDef, List<(string StatDef, float Value)> offsets)
		=> offsets.ForEach(t => Add(thingDef, t.StatDef, t.Value));

	public void ExposeData() {
		var tuples = Scribe.mode == LoadSaveMode.Saving ? ((IEnumerable<Tuple>)this).Select(t => (ThingStatOffset)t).ToList() : [];
		Scribe_Collections.Look(ref tuples, "entries", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars) {
			Clear();
			foreach (var tuple in tuples) {
				if (!tuple.Valid) { // Possible invalid data from save file
					Logger.Warning("Invalid stat offset");
					continue;
				}
				Add(tuple);
			}
		}
	}

	public ThingStatOffsetCollection Clone() => [.. this];

	public ThingStatOffsetCollection Transform(Func<float, float> selector) => Transform(selector, _ => true);

	public ThingStatOffsetCollection Transform(Func<float, float> selector, Func<Tuple, bool> filter) {
		var updates = this.Where(filter)
			.Select(tuple => (tuple.Item1, tuple.Item2, selector(tuple.Item3)))
			.ToList();
		foreach (var (thingDef, statDef, value) in updates)
			this[thingDef, statDef] = value;
		return this;
	}


	public void Apply(ApplyMode mode = ApplyMode.Set) 
		=> Apply(t => t.equippedStatOffsets ??= [], mode);

	public void Apply(Func<ThingDef, List<StatModifier>> listGetter, ApplyMode mode = ApplyMode.Set) {
		foreach (var group in this.GroupBy(t => t.Item1)) {
			var thing = DefDatabase<ThingDef>.GetNamed(group.Key, false);
			if (thing is null) {
				Logger.Warning($"Thing {group.Key} not found", true);
				continue;
			}
			var statList = listGetter(thing);
			var modifiers = group.Select(t => (StatModifier)(ThingStatOffset)t).ToArray();
			if (mode == ApplyMode.Overwrite)
				statList.Clear();
			else if (mode == ApplyMode.Set) {
				var stats = new HashSet<string>(modifiers.Select(o => o.stat.defName));
				statList.RemoveAll(m => stats.Contains(m.stat.defName));
			}
			statList.AddRange(modifiers);
		}
	}
}
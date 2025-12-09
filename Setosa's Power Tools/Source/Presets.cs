namespace Setosa;

public static class Presets {
	public static readonly ThingStatOffsetCollection Normal = new() {
		{
			"AdvancedTool_IndustrialChainsaw",
			[
				("PlantWorkSpeed", 0.8f),
				("ButcheryFleshSpeed", 0.4f),
				("MoveSpeed", -0.25f)
			]
		}, {
			"AdvancedTool_PowerSaw",
			[
				("ConstructionSpeed", 0.8f),
				("MoveSpeed", -0.25f)
			]
		}, {
			"AdvancedTool_PowerAuger",
			[
				("PlantWorkSpeed", 1.2f),
				("MoveSpeed", -0.25f)
			]
		}, {
			"AdvancedTool_Chisel",
			[
				("GeneralLaborSpeed", 0.4f),
				("SmoothingSpeed", 0.8f),
				("ConstructionSpeed", 0.2f)
			]
		}, {
			"AdvancedTool_Jackhammer",
			[
				("MiningSpeed", 0.8f),
				("MoveSpeed", -0.25f)
			]
		}, {
			"AdvancedTool_Drill",
			[
				("GeneralLaborSpeed", 0.8f),
				("ConstructionSpeed", 0.2f),
				("ButcheryMechanoidEfficiency", 0.8f),
				("ButcheryMechanoidSpeed", 0.8f)
			]
		}, {
			"AdvancedTool_NailGun",
			[("ConstructionSpeed", 0.4f)]
		}
	};

	public static readonly ThingStatOffsetCollection Nerfed = Normal.Clone().Transform(v => v > 0 ? v / 2f : v);

	public static readonly ThingStatOffsetCollection Buffed = Normal.Clone().Transform(v => v > 0 ? v * 1.5f : v);
}

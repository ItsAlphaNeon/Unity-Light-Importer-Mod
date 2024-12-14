using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;

namespace UnityLightImporterMod;
public class UnityLightImporterMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "UnityLightImporterMod";
	public override string Author => "AlphaNeon";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/resonite-modding-group/UnityLightImporterMod/";

	public override void OnEngineInit() {

	}
}

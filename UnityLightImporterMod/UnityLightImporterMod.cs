using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace UnityLightImporterMod {
	public class UnityLightImporterMod : ResoniteMod {
		internal const string VERSION_CONSTANT = "1.0.0";

		public override string Author => "AlphaNeon";
		public override string Name => "UnityLightImporterMod";
		public override string Version => VERSION_CONSTANT;
		public override string Link => "https://github.com/resonite-modding-group/UnityLightImporterMod/";

		public class LightData {
			public List<LightInfo>? Lights { get; set; }
		}

		public class LightInfo {
			public Position? GlobalPosition { get; set; }
			public Quaternion? Rotation { get; set; }
			public string? LightType { get; set; }
			public float Intensity { get; set; }
			public string? Color { get; set; }
			public float Range { get; set; }
			public float SpotAngle { get; set; }
			public bool Enabled { get; set; }
			public string? ShadowType { get; set; }
		}

		public class Position {
			public float x { get; set; }
			public float y { get; set; }
			public float z { get; set; }
		}

		public class Quaternion {
			public float x { get; set; }
			public float y { get; set; }
			public float z { get; set; }
			public float w { get; set; }
		}

		static void LightImporterUI(Slot slot) {
			UIBuilder ui = RadiantUI_Panel.SetupPanel(slot, (LocaleString)"Unity Light Importer", new float2(600f, 400f));
			slot.PersistentSelf = false;
			slot.LocalScale *= 0.0009f;
			ui.ScrollArea();
			ui.VerticalLayout(4f, 8f, childAlignment: Alignment.TopLeft);
			ui.FitContent(SizeFit.Disabled, SizeFit.MinSize);
			ui.Style.MinHeight = 32f;
			TextField jsonInputField = ui.TextField("Paste JSON Here", true, "Undo modify TextField");
			Button runButton = ui.Button("Run Light Importer");
			runButton.LocalPressed += (button, eventData) => RunImport(button, eventData, slot, jsonInputField.TargetString);
		}

		static void RunImport(IButton button, ButtonEventData eventData, Slot s, string json) {
			LightImporterRun(s, json);
		}

		static void LightImporterRun(Slot lightRootSlot, string json) {
			LightData lightData;
			try {
				lightData = JsonConvert.DeserializeObject<LightData>(json) ?? throw new JsonException("Deserialized object is null");
			} catch (JsonException ex) {
				Error("Error parsing JSON: ", ex.Message);
				return;
			}

			if (lightData.Lights == null) // Compiler gets pissy if I don't do this
			{
				Error("No lights data found in the JSON.");
				return;
			}

			foreach (var lightInfo in lightData.Lights) // Same here. Thanks C#
{
				try {
					Slot lightSlot = lightRootSlot.Parent.AddSlot($"{lightInfo.LightType} Light" + "<color=#" + lightInfo.Color + "> ⏹</color>");
					if (lightInfo.GlobalPosition != null && lightInfo.Rotation != null) { // We love null checks
						lightSlot.GlobalPosition = new float3(lightInfo.GlobalPosition.x, lightInfo.GlobalPosition.y, lightInfo.GlobalPosition.z);
						lightSlot.GlobalRotation = new floatQ(lightInfo.Rotation.x, lightInfo.Rotation.y, lightInfo.Rotation.z, lightInfo.Rotation.w);
					} else {
						throw new InvalidOperationException("GlobalPosition is null for one of the lights.");
					}
					// Lighting Light Type
					Light lightComponent = lightSlot.AttachComponent<Light>();
					switch (lightInfo.LightType) {
						case "Directional":
							lightComponent.LightType.Value = LightType.Directional;
							break;
						case "Spot":
							lightComponent.LightType.Value = LightType.Spot;
							lightComponent.SpotAngle.Value = lightInfo.SpotAngle;
							break;
						case "Point":
							lightComponent.LightType.Value = LightType.Point;
							break;
						default:
							lightComponent.Destroy();
							Error($"Unknown light type: {lightInfo.LightType}");
							continue;
					}
					// Lighting Color
					if (lightInfo.Color != null) {
						string hex = lightInfo.Color;
						var (r, g, b, a) = HexToLinearRGBA(hex);
						lightComponent.Color.Value = new colorX((float)r, (float)g, (float)b, (float)a);
					} else {
						lightComponent.Color.Value = new colorX(1, 1, 1, 1);
					}
					//Lighting Shadow Type
					lightComponent.ShadowType.Value = lightInfo.ShadowType switch {
						"Hard" => ShadowType.Hard,
						"Soft" => ShadowType.Soft,
						"None" => ShadowType.None,
						_ => throw new ArgumentException($"Unknown shadow type: {lightInfo.ShadowType}")
					};
					lightComponent.Intensity.Value = lightInfo.Intensity;
					lightComponent.Range.Value = lightInfo.Range;
					lightComponent.Enabled = lightInfo.Enabled;
				} catch (Exception e) {
					Error($"An error occured when processing light at {lightInfo.GlobalPosition}: ", e.Message);
					continue;
				}
			}
		}


		public static (double R, double G, double B, double A) HexToLinearRGBA(string hex) {
			if (hex.Length == 8) {
				var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
				var a = int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

				var linearR = r / 255.0;
				var linearG = g / 255.0;
				var linearB = b / 255.0;
				var linearA = a / 255.0;

				return (linearR, linearG, linearB, linearA);
			} else {
				throw new ArgumentException("Invalid length for the hex color, should be 8 characters length.");
			}
		}


		public override void OnEngineInit() {
			Engine.Current.RunPostInit(() => {
				DevCreateNewForm.AddAction("Editor", "Unity Light Importer (Mod)", LightImporterUI);
			});
		}
	}
}

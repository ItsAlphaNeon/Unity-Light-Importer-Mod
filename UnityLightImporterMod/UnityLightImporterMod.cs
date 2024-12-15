using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

// Some implementations were inspired by 989onan's ResoniteBakery mod.

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
				Slot lightSlot = lightRootSlot.Parent.AddSlot($"{lightInfo.LightType} Light");
				if (lightInfo.GlobalPosition != null) {
					lightSlot.GlobalPosition = new float3(lightInfo.GlobalPosition.x, lightInfo.GlobalPosition.y, lightInfo.GlobalPosition.z);
				} else {
					Error("GlobalPosition is null for one of the lights.");
					continue;
				}

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
					case "Area":
						Error("Area lights are not supported in FrooxEngine.");
						continue;
					default:
						throw new ArgumentException($"Unknown light type: {lightInfo.LightType}");
				}
				// Figuring out this color stuff was a nighmare. REEEEE
				if (lightInfo.Color != null) {
					string hex = lightInfo.Color;
					if (hex.Length == 6) {
						hex = "FF" + hex;
					}

					try {
						byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
						byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
						byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
						byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
						lightComponent.Color.Value = new colorX(r / 255f, g / 255f, b / 255f, a / 255f);
					} catch (FormatException) {
						Error($"Invalid color format for light with type {lightInfo.LightType}. Defaulting to white.");
						lightComponent.Color.Value = new colorX(1, 1, 1, 1);
					}
				} else {
					lightComponent.Color.Value = new colorX(1, 1, 1, 1);
				}

				lightComponent.Intensity.Value = lightInfo.Intensity;
				lightComponent.Range.Value = lightInfo.Range;
				lightComponent.Enabled = lightInfo.Enabled;
			}
		}

		

		public override void OnEngineInit() {
			Engine.Current.RunPostInit(() => {
				DevCreateNewForm.AddAction("Editor", "Unity Light Importer (Mod)", LightImporterUI);
			});
		}
	}
}

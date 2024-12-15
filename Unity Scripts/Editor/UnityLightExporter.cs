using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ExportLightsToJson
{
    [MenuItem("Tools/Export Lights to JSON")]
    private static void ExportLights()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        List<LightData> lightsInfo = new List<LightData>();

        foreach (Light light in lights)
        {
            LightData lightInfo = new LightData
            {
                GlobalPosition = new Vector3(light.transform.position.x, light.transform.position.y, light.transform.position.z),
                Rotation = new Vector4(light.transform.rotation.x, light.transform.rotation.y, light.transform.rotation.z, light.transform.rotation.w),
                LightType = light.type.ToString(),
                Intensity = light.intensity,
                Color = ColorUtility.ToHtmlStringRGBA(light.color),
                Range = light.range,
                SpotAngle = light.type == LightType.Spot ? light.spotAngle : 0.0f,
                Enabled = light.enabled,
                ShadowType = light.shadows.ToString()
            };

            lightsInfo.Add(lightInfo);
        }

        string jsonOutput = JsonUtility.ToJson(new LightDataWrapper { lights = lightsInfo }, true);

        string path = Application.dataPath + "/LightsExport.txt";
        File.WriteAllText(path, jsonOutput);
        Debug.Log("Lights exported to " + path);
        Debug.Log("Number of lights exported: " + lights.Length);
    }

    [System.Serializable]
    private class LightData
    {
        public Vector3 GlobalPosition;
        public Vector4 Rotation;
        public string LightType;
        public float Intensity;
        public string Color;
        public float Range;
        public float SpotAngle;
        public bool Enabled;
        public string ShadowType;
    }

    [System.Serializable]
    private class LightDataWrapper
    {
        public List<LightData> lights;
    }
}

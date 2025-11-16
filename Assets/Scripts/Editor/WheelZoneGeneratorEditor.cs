using UnityEngine;
using UnityEditor;
using System.IO;

public class WheelZoneGeneratorEditor
{
    private const int ZoneCount = 30;
    private const string FolderPath = "Assets/GameData/Zones/";

    [MenuItem("Tools/Wheel/Generate All Zones")]
    public static void GenerateAllZones()
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        for (int i = 1; i <= ZoneCount; i++)
        {
            string path = FolderPath + $"zone_{i.ToString("D3")}.asset";

            WheelZoneSO zone = ScriptableObject.CreateInstance<WheelZoneSO>();

            // Zone index
            SetZoneIndex(zone, i);

            // Flags
            SetFlags(zone, i);

            // Save
            AssetDatabase.CreateAsset(zone, path);
            EditorUtility.SetDirty(zone);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("All 30 Wheel Zones generated successfully!");
    }

    private static void SetZoneIndex(WheelZoneSO zone, int index)
    {
        var indexField = typeof(WheelZoneSO)
            .GetField("_zoneIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        indexField.SetValue(zone, index);
    }

    private static void SetFlags(WheelZoneSO zone, int index)
    {
        var safeField = typeof(WheelZoneSO)
            .GetField("_isSafeZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var superField = typeof(WheelZoneSO)
            .GetField("_isSuperZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        bool isSafe = (index % 5 == 0 && index != 30);
        bool isSuper = (index == 30);

        safeField.SetValue(zone, isSafe);
        superField.SetValue(zone, isSuper);
    }
}
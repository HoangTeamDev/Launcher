using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;

public class AddressablesBuildTool : EditorWindow
{
    [MenuItem("Ninja/Tools/Addressables Build Tool")]
    public static void ShowWindow()
    {
        GetWindow<AddressablesBuildTool>("Addressables Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("📦 Addressables Build Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("🧹 Clean Build Cache"))
        {
            CleanBuild();
        }

        if (GUILayout.Button("🚀 Build New (Player Content)"))
        {
            BuildNew();
        }

        if (GUILayout.Button("🔍 Check for Content Update"))
        {
            CheckContentUpdate();
        }

        if (GUILayout.Button("♻️ Build Content Update"))
        {
            BuildUpdate();
        }
        if (GUILayout.Button("♻️ CleanKey"))
        {
            CleanKeys();
        }

        /*GUILayout.Space(10);
        GUILayout.Label("⚙️  Settings & Utilities", EditorStyles.boldLabel);

        if (GUILayout.Button("📁 Open Addressables Settings"))
        {
            Selection.activeObject = AddressableAssetSettingsDefaultObject.Settings;
        }

        if (GUILayout.Button("📂 Open Build Log Folder"))
        {
            string logPath = Path.Combine(Addressables.BuildPath, "addressables_content_state.bin");
            EditorUtility.RevealInFinder(logPath);
        }*/
       /* GUILayout.Space(10);
        GUILayout.Label("📁 Quick Open Addressable Groups", EditorStyles.boldLabel);

        if (GUILayout.Button("📦 Select 'BoxCamera' Group")) SelectGroup("BoxCamera");
        if (GUILayout.Button("🗺 Select 'Map' Group")) SelectGroup("Map");
        if (GUILayout.Button("👤 Select 'NPC' Group")) SelectGroup("NPC");
        if (GUILayout.Button("👾 Select 'Enemy' Group")) SelectGroup("Enemy");*/
        GUILayout.Space(10);
        GUILayout.Label("🌐 Addressables Group Switch", EditorStyles.boldLabel);

        if (GUILayout.Button("🔁 Set All Groups to Remote"))
        {
            SwitchGroupsTo("Remote");
        }
        if (GUILayout.Button("📦 Set All Groups to Local"))
        {
            SwitchGroupsTo("Local");
        }
        
        
        GUILayout.Label("▶️ Play Mode Script", EditorStyles.boldLabel);

        // Lấy danh sách builder
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var currentIndex = settings.ActivePlayModeDataBuilderIndex;

        for (int i = 0; i < settings.DataBuilders.Count; i++)
        {
            var builder = settings.DataBuilders[i];
            if (builder == null) continue;

            bool selected = (i == currentIndex);
            string label = builder.name;

            if (GUILayout.Toggle(selected, label, "Button"))
            {
                if (currentIndex != i)
                {
                    settings.ActivePlayModeDataBuilderIndex = i;
                    Debug.Log($"✅ Play Mode Script set to: {label}");
                }
            }
        }

    }
    public  void CleanKeys()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("AddressableAssetSettings not found!");
            return;
        }

        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                string fileName = Path.GetFileNameWithoutExtension(path);

                if (entry.address != fileName)
                {
                    Debug.Log($"Key changed: {entry.address} -> {fileName}");
                    entry.SetAddress(fileName);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Addressable keys cleaned!");
    }
    private void CleanBuild()
    {
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        Debug.Log("✅ Addressables Build Cache Cleaned");
    }

    private void BuildNew()
    {
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("✅ Addressables Build Completed");
    }

    private void CheckContentUpdate()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var previousPath = Addressables.BuildPath + "/addressables_content_state.bin";

        if (!File.Exists(previousPath))
        {
            Debug.LogWarning("⚠️ No previous content state found. Build at least once before checking update.");
            return;
        }

        var modifiedEntries = ContentUpdateScript.GatherModifiedEntries(settings, previousPath);
        Debug.Log($"🔍 {modifiedEntries.Count} modified entries found.");
    }

    private void BuildUpdate()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var previousPath = Addressables.BuildPath + "/addressables_content_state.bin";

        if (!File.Exists(previousPath))
        {
            Debug.LogWarning("⚠️ Cannot build update. No previous build state file found.");
            return;
        }

        ContentUpdateScript.BuildContentUpdate(settings, previousPath);
        Debug.Log("♻️ Content Update Build Complete.");
    }
    private void SelectGroup(string groupName)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var group in settings.groups)
        {
            if (group != null && group.Name == groupName)
            {
                Selection.activeObject = group;
                EditorGUIUtility.PingObject(group);
                Debug.Log($"📂 Selected Addressable Group: {groupName}");
                return;
            }
        }
        Debug.LogWarning($"⚠️ Addressable Group '{groupName}' not found.");
    }
    private void SwitchGroupsTo(string mode)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string defaultGroupName = "Default Local Group (Default)";

        // Lấy chuỗi tên biến trong profile
        string buildPath = mode == "Remote" ? "Remote.BuildPath" : "Local.BuildPath";
        string loadPath = mode == "Remote" ? "Remote.LoadPath" : "Local.LoadPath";

        foreach (var group in settings.groups)
        {
            if (group == null || group.ReadOnly || group.Name == defaultGroupName) continue;

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null) continue;

            schema.BuildPath.SetVariableByName(settings, buildPath);
            schema.LoadPath.SetVariableByName(settings, loadPath);

            EditorUtility.SetDirty(group);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Switched all groups (except default) to: {mode.ToUpper()} mode");
    }
    private void AddMissingToGroup(string groupName, string folderPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            Debug.LogError($"❌ Group '{groupName}' không tồn tại.");
            return;
        }

        // Đảm bảo đường dẫn hợp lệ
        if (!folderPath.StartsWith("Assets"))
        {
            Debug.LogError("❌ Đường dẫn thư mục phải bắt đầu bằng 'Assets/'");
            return;
        }

        // Quét tất cả prefab trong thư mục
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        int added = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // Kiểm tra xem asset này đã có trong addressable chưa
            if (settings.FindAssetEntry(guid) != null)
                continue;

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.SetAddress(assetName); // Tự động đặt key theo tên file
            entry.SetLabel("Preload", true); // Thêm label nếu cần
            added++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Đã thêm {added} asset còn thiếu vào group '{groupName}'.");
    }


}

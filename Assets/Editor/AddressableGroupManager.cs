using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas; // <-- dùng schema
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings.GroupSchemas; // nhớ import
public class AddressableGroupManager : EditorWindow
{
    private AddressableAssetSettings settings;
    private AddressableAssetGroup selectedGroup;
    private Vector2 scroll;

    // --- Search ---
    private SearchField searchField;
    private string searchText = "";
    private bool searchByPath = true;

    // --- Create Group ---
    private string newGroupName = "New_Group";
    private bool addBundledSchema = true;
    private bool addContentUpdateSchema = true;
    private bool setAsDefault = false;

    [MenuItem("Ninja/Tools/Addressables/Group Manager")]
    public static void ShowWindow()
    {
        GetWindow<AddressableGroupManager>("Addressable Group Tool");
    }

    private void OnEnable()
    {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        searchField = new SearchField();
    }

    private void OnGUI()
    {
        if (settings == null)
        {
            EditorGUILayout.HelpBox("Không tìm thấy AddressableAssetSettings.", MessageType.Error);
            return;
        }

        // ====== TẠO GROUP MỚI ======
        EditorGUILayout.LabelField("Tạo Group Mới", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        newGroupName = EditorGUILayout.TextField("🐱‍🐉 Tên group", newGroupName);
        addBundledSchema = EditorGUILayout.ToggleLeft("Thêm BundledAssetGroupSchema", addBundledSchema);
        addContentUpdateSchema = EditorGUILayout.ToggleLeft("Thêm ContentUpdateGroupSchema", addContentUpdateSchema);
        setAsDefault = EditorGUILayout.ToggleLeft("Đặt làm Default Group", setAsDefault);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newGroupName)))
        {
            if (GUILayout.Button("🐱‍🏍Tạo Group"))
            {
                CreateNewGroup();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ====== CHỌN GROUP ======
        EditorGUILayout.LabelField("Chọn Group:", EditorStyles.boldLabel);
        foreach (var group in settings.groups)
        {
            if (group == null) continue;
            if (GUILayout.Button($"💾{group.Name}"))
            {
                selectedGroup = group;
                searchText = "";
                Repaint();
            }
        }

        EditorGUILayout.Space();

        if (selectedGroup != null)
        {
            GUIStyle centeredBold = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            EditorGUILayout.LabelField("Thêm Vào Group: " + selectedGroup.Name, centeredBold, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("🎆Thêm"))
            {
                foreach (var obj in Selection.objects)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (!string.IsNullOrEmpty(guid))
                        settings.CreateOrMoveEntry(guid, selectedGroup);
                }
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space();

            // --- THANH TÌM KIẾM ---
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Tìm trong group", GUILayout.Width(100));
            Rect searchRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
            searchText = searchField.OnGUI(searchRect, searchText);
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                searchText = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            searchByPath = EditorGUILayout.ToggleLeft("Tìm theo tên file (path) ngoài Address", searchByPath);

            EditorGUILayout.LabelField($"Group: {selectedGroup.Name}", EditorStyles.boldLabel);

            // Lọc danh sách theo search
            var entries = selectedGroup.entries;
            if (!string.IsNullOrEmpty(searchText))
            {
                string needle = searchText.ToLowerInvariant();
                entries = entries.Where(e =>
                {
                    bool byAddress = !string.IsNullOrEmpty(e.address) && e.address.ToLowerInvariant().Contains(needle);
                    if (!searchByPath) return byAddress;

                    string path = AssetDatabase.GUIDToAssetPath(e.guid);
                    string fileName = Path.GetFileNameWithoutExtension(path) ?? "";
                    bool byFile = fileName.ToLowerInvariant().Contains(needle);
                    return byAddress || byFile;
                }).ToList();
            }

            EditorGUILayout.LabelField($"Kết quả: {entries.Count} item(s)");

            // Danh sách entries
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(100));
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                EditorGUILayout.BeginHorizontal();

                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                string fileName = Path.GetFileName(path);

                EditorGUILayout.LabelField(entry.address, GUILayout.MinWidth(50));
              

                if (GUILayout.Button("🔎", GUILayout.Width(45)))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    EditorGUIUtility.PingObject(obj);
                }
                if (GUILayout.Button("🗑", GUILayout.Width(50)))
                {
                    selectedGroup.RemoveAssetEntry(entry);
                    AssetDatabase.SaveAssets();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void CreateNewGroup()
    {
        string name = newGroupName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            EditorUtility.DisplayDialog("Tên không hợp lệ", "Vui lòng nhập tên group.", "OK");
            return;
        }
        if (settings.groups.Any(g => g != null && g.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
        {
            EditorUtility.DisplayDialog("Trùng tên", $"Đã tồn tại group '{name}'.", "OK");
            return;
        }

        // Tạo các schema INSTANCE (không phải Type)
        var schemaObjs = new System.Collections.Generic.List<AddressableAssetGroupSchema>();
        if (addBundledSchema)
        {
            var bundled = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            // (tuỳ chọn) cấu hình mặc định cho bundled ở đây, ví dụ:
            // bundled.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schemaObjs.Add(bundled);
        }
        if (addContentUpdateSchema)
        {
            var contentUpdate = ScriptableObject.CreateInstance<ContentUpdateGroupSchema>();
            schemaObjs.Add(contentUpdate);
        }

        // ✅ Overload cần IEnumerable<AddressableAssetGroupSchema>
        var group = settings.CreateGroup(
            name,          // groupName
            setAsDefault,  // setAsDefaultGroup
            false,         // readOnly
            true,          // postEvent
            schemaObjs     // IEnumerable<AddressableAssetGroupSchema>
        );

        selectedGroup = group;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Repaint();
    }

}

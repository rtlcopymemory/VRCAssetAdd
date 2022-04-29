using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Assets.VRCAssetAdd.Editor;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class AssetCreate : EditorWindow
{
    private static readonly string ModelKey = "\"model\"";
    private static readonly string AssetKey = "\"asset\"";

    [MenuItem("VRCAssetAdd/Asset Create")]
    public static void ShowWindow()
    {
        AssetCreate wnd = GetWindow<AssetCreate>();
        wnd.titleContent = new GUIContent("Asset Create");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/VRCAssetAdd/Editor/AssetCreate.uxml");
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/VRCAssetAdd/Editor/AssetCreate.uss");
        root.styleSheets.Add(styleSheet);

        var duplicateBtn = root.Q<Button>("duplicate-button");
        duplicateBtn.RegisterCallback<MouseUpEvent>((evt) => { DuplicateAvatar(); });

        var generateButton = root.Q<Button>("generate-button");
        generateButton.RegisterCallback<MouseUpEvent>((evt) => { GenerateFiles(); });

        // Assign type to ObjectFields
        var avatarField = rootVisualElement.Q<ObjectField>("avatar-field");
        avatarField.objectType = typeof(GameObject);

        var assetField = rootVisualElement.Q<ObjectField>("asset-field");
        assetField.objectType = typeof(GameObject);
        
        var targetField = rootVisualElement.Q<ObjectField>("target-field");
        targetField.objectType = typeof(GameObject);
    }

    private void DuplicateAvatar()
    {
        var avatarField = rootVisualElement.Q<ObjectField>("avatar-field");
        var targetField = rootVisualElement.Q<ObjectField>("target-field");
        var assetField = rootVisualElement.Q<ObjectField>("asset-field");
        var avatar = avatarField.value as GameObject;
        var asset = assetField.value as GameObject;

        if (avatar == null || asset == null) return;

        var target = Instantiate(avatar);
        var targetAsset = Instantiate(asset);

        target.transform.position = new Vector3(target.transform.position.x + 2, target.transform.position.y, target.transform.position.z);
        target.name = avatar.name + " - Target";
        targetField.value = target;

        targetAsset.transform.position = new Vector3(targetAsset.transform.position.x + 2, targetAsset.transform.position.y, targetAsset.transform.position.z);
        targetAsset.name = asset.name + " - Target";
    }

    private void GenerateFiles()
    {
        // iterate through hierarchy on both objects at the same time
        var avatarField = rootVisualElement.Q<ObjectField>("avatar-field");
        var assetField = rootVisualElement.Q<ObjectField>("asset-field");
        var targetField = rootVisualElement.Q<ObjectField>("target-field");
        var target = targetField.value as GameObject;
        var asset = assetField.value as GameObject;
        var avatar = avatarField.value as GameObject;

        if (avatar == null || asset == null || target == null) return;

        var searcher = new AssetSearch(target.transform, asset.transform, avatar.transform);
        var differences = searcher.Search();

        // Sorting from deeper to less deep. This is needed so it won't move the root of the asset before the rest
        differences.Sort((a, b) => a.AssetPath.Split('/').Length.CompareTo(b.AssetPath.Split('/').Length));
        differences.Reverse();
        
        var json = JsonHelper.ToJson(differences.ToArray());

        var path = EditorUtility.SaveFilePanel(
            "Save VRCAssetAdd File",
            "",
            asset.name + "_AssetDiff.json",
            "json");

        if (path.Length == 0)
        {
            Debug.LogError("Path was 0");
            return;
        }

        File.WriteAllText(path, json);
    }

    
}
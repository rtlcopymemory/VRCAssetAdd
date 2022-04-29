using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using Assets.VRCAssetAdd.Editor;


public class AssetApply : EditorWindow
{
    [MenuItem("VRCAssetAdd/Apply From File")]
    public static void OpenWindow()
    {
        AssetApply wnd = GetWindow<AssetApply>();
        wnd.titleContent = new GUIContent("Apply From File");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/VRCAssetAdd/Editor/AssetApply.uxml");
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/VRCAssetAdd/Editor/AssetApply.uss");
        root.styleSheets.Add(styleSheet);

        var duplicateBtn = root.Q<Button>("apply-button");
        duplicateBtn.RegisterCallback<MouseUpEvent>((evt) => { Apply(); });

        var avatarField = root.Q<ObjectField>("avatar-field");
        avatarField.objectType = typeof(GameObject);

        var assetField = root.Q<ObjectField>("asset-field");
        assetField.objectType = typeof(GameObject);
    }

    private void Apply()
    {
        var avatarField = rootVisualElement.Q<ObjectField>("avatar-field");
        var assetField = rootVisualElement.Q<ObjectField>("asset-field");

        var avatar = avatarField.value as GameObject;
        var asset = assetField.value as GameObject;

        if (avatar == null || asset == null)
        {
            ShowError("Avatar or Asset not selected");
            return;
        }

        var avatarBak = Instantiate(avatar);
        var assetBak = Instantiate(asset);

        avatarBak.name = $"{avatar.name} Backup";
        assetBak.name = $"{asset.name} Backup";

        avatarBak.SetActive(false);
        assetBak.SetActive(false);

        if (PrefabUtility.IsAnyPrefabInstanceRoot(avatar))
            PrefabUtility.UnpackPrefabInstance(avatar, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        if (PrefabUtility.IsAnyPrefabInstanceRoot(asset))
            PrefabUtility.UnpackPrefabInstance(asset, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        string path = EditorUtility.OpenFilePanel("Open JSON Diffs", "", "json");
        string jsonStirng = File.ReadAllText(path);
        var differences = JsonHelper.FromJson<AssetDifference>(jsonStirng);

        foreach (var difference in differences)
        {
            var avatar_go = GameObject.Find(ToNewRoot(difference.ModelPath, avatar.name));
            var asset_go = GameObject.Find(ToNewRoot(difference.AssetPath, asset.name));

            if (avatar_go == null)
                ShowError($"Avatar GameObject could not be found: {ToNewRoot(difference.ModelPath, avatar.name)}");
            
            if (asset_go == null)
                ShowError($"Asset GameObject could not be found: {ToNewRoot(difference.AssetPath, asset.name)}");

            asset_go.transform.parent = avatar_go.transform;
        }
    }

    private string ToNewRoot(string fromFile, string rootName)
    {
        if (fromFile == null || rootName == null) {
            ShowError($"Something was null!\nfromFile: {fromFile} - rootName: {rootName}");
            return null;
        }

        var parts = fromFile.Split('/');
        return fromFile.Replace(parts[0], rootName);
    }

    private void ShowError(string message)
    {
        var oldErr = rootVisualElement.Q("Error");
        if (oldErr != null)
            rootVisualElement.Remove(oldErr);

        var msg = new Label(message)
        {
            name = "Error"
        };
        rootVisualElement.Add(msg);
    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Assets.VRCAssetAdd;
using Assets.VRCAssetAdd.Editor;
using System.IO;

public class BlendshapeTransfer : EditorWindow
{
    [MenuItem("VRCAssetAdd/Blendshape Transfer")]
    public static void ShowWindow()
    {
        BlendshapeTransfer wnd = GetWindow<BlendshapeTransfer>();
        wnd.titleContent = new GUIContent("Blendshape Transfer");
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/VRCAssetAdd/Editor/BlendshapeTransfer.uxml");
        visualTree.CloneTree(rootVisualElement);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/VRCAssetAdd/Editor/BlendshapeTransfer.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        SetupComponents();
    }

    private void SetupComponents()
    {
        var in_mesh = rootVisualElement.Q<ObjectField>("in_mesh");
        var out_mesh = rootVisualElement.Q<ObjectField>("out_mesh");

        in_mesh.objectType = typeof(GameObject);
        out_mesh.objectType = typeof(GameObject);

        var popup_container = rootVisualElement.Q<VisualElement>("popup-container");
        var popoutField = new PopupField<string>("Blend Shape to export", new List<string>() { "Select a Mesh" }, 0);
        popoutField.name = "shapes-popout";
        popoutField.SetEnabled(false);
        popup_container.Add(popoutField);

        out_mesh.RegisterValueChangedCallback((evt) => { HandleMeshChange(); });

        var out_btn = rootVisualElement.Q<Button>("out_btn");
        var in_btn = rootVisualElement.Q<Button>("in_btn");

        out_btn.RegisterCallback<MouseUpEvent>((evt) => { HandleExport(); });
        in_btn.RegisterCallback<MouseUpEvent>((evt) => { HandleImport(); });
    }

    private void HandleMeshChange()
    {
        var out_mesh = rootVisualElement.Q<ObjectField>("out_mesh");
        var mesh_go = out_mesh.value as GameObject;

        
        if (!mesh_go.TryGetComponent<SkinnedMeshRenderer>(out var mr))
        {
            ShowError("Selected object did not have a skinned mesh renderer");
            return;
        }

        var mesh = mr.sharedMesh;
        var blendShapes = GetBlendshapeNames(mesh);

        var popup_container = rootVisualElement.Q<VisualElement>("popup-container");
        var popupField = rootVisualElement.Q<PopupField<string>>("shapes-popout");
        popup_container.Remove(popupField);

        var popoutField = new PopupField<string>("Blend Shape to export", blendShapes, 0);
        popoutField.name = "shapes-popout";
        popoutField.SetEnabled(true);
        popup_container.Add(popoutField);
    }

    private void HandleExport()
    {
        ClearTexts();
        var popupField = rootVisualElement.Q<PopupField<string>>("shapes-popout");
        var blendshapeName = popupField.value;

        var out_mesh = rootVisualElement.Q<ObjectField>("out_mesh");
        var mesh_go = out_mesh.value as GameObject;

        
        if (!mesh_go.TryGetComponent<SkinnedMeshRenderer>(out var mr))
        {
            ShowError("Selected object did not have a skinned mesh renderer");
            return;
        }

        var mesh = mr.sharedMesh;
        var shapeindex = mesh.GetBlendShapeIndex(blendshapeName);
        var frameCount = mesh.GetBlendShapeFrameCount(shapeindex);

        Vector3[] vertices = new Vector3[mesh.vertexCount];
        Vector3[] normals = new Vector3[mesh.vertexCount];
        Vector3[] tangents = new Vector3[mesh.vertexCount];
        mr.sharedMesh.GetBlendShapeFrameVertices(shapeindex, frameCount - 1, vertices, normals, tangents);

        List<int> affectedVerticies = new List<int>();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (DeltasHaveDifferences(vertices[i], normals[i], tangents[i]))
            {
                affectedVerticies.Add(i);
                Debug.Log("Normal deltas: (" + normals[i].x + ", " + normals[i].y + ", " + normals[i].z + ")");
            }
        }

        var IdVerts = IdentifyVerticiesFromIndex(affectedVerticies, mesh);
        var vertDeltas = RemapDeltasArray(vertices, affectedVerticies);
        var normDeltas = RemapDeltasArray(normals, affectedVerticies);
        var tanDeltas = RemapDeltasArray(tangents, affectedVerticies);

        if (vertDeltas.Length != IdVerts.Count)
            throw new VRCAddException("Mismatches length of IDVerts and Deltas");

        var bsFile = new VRCABlendShape(blendshapeName, mesh_go.transform.localScale, IdVerts.ToArray(), vertDeltas, normDeltas, tanDeltas);

        var path = EditorUtility.SaveFilePanel(
            "Save BlendShapeTransfer File",
            "",
            blendshapeName + "_BlendShape.bst",
            "bst");

        if (path.Length == 0)
        {
            ShowError("Path was 0");
            return;
        }

        File.WriteAllBytes(path, bsFile.ConvertToBytes());
    }

    private bool DeltasHaveDifferences(Vector3 vertex, Vector3 normal, Vector3 tangent)
    {
        return normal.x != 0 || normal.y != 0 || normal.z != 0 
            || vertex.x != 0 || vertex.y != 0 || vertex.z != 0 
            || tangent.x != 0 || tangent.y != 0 || tangent.z != 0;
    }

    /// <summary>
    /// Identify verticies by position + 1 triangle that contains them  
    /// This is not perfect but should be good for most cases.  
    /// It's also fairly heavy computationally as I have to check every triangle  
    /// 
    /// Potentially O(n*m) with n = verticies and m = triangles  
    /// Realistically a blendshape that affects the whole body exists but it rare-ish? I hope
    /// </summary>
    /// <param name="indeces">A list of indices to identify</param>
    /// <param name="mesh">the source mesh</param>
    private List<VertexIdentifier> IdentifyVerticiesFromIndex(List<int> indeces, Mesh mesh)
    {
        var res = new List<VertexIdentifier>();
        var modelMap = new ModelMapper(mesh);
        foreach (var i in indeces)
        {
            var vertID = new VertexIdentifier
            {
                Position = mesh.vertices[i],
                Triangle = modelMap.FindTriangleContaining(i)
            };

            res.Add(vertID);
        }

        return res;
    }

    private Vector3[] RemapDeltasArray(Vector3[] deltaArray, List<int> indexes)
    {
        Vector3[] vertDeltas = new Vector3[indexes.Count];

        for (int i = 0; i < vertDeltas.Length; i++)
        {
            vertDeltas[i] = deltaArray[indexes[i]];
        }

        return vertDeltas;
    }

    private List<string> GetBlendshapeNames(Mesh mesh)
    {
        var res = new List<string>();

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            res.Add(mesh.GetBlendShapeName(i));
        }

        return res;
    }

    private void HandleImport()
    {
        ClearTexts();
        ShowWarning("This operation can take minutes");
        var path = EditorUtility.OpenFilePanel("Open BlendShapeTransfer File", null, "bst");

        if (path.Length == 0)
        {
            ShowError("No File Selected");
            return;
        }

        var in_mesh = rootVisualElement.Q<ObjectField>("in_mesh");
        var mesh_go = in_mesh.value as GameObject;

        var mr = mesh_go.GetComponent<SkinnedMeshRenderer>();

        if (mr == null)
        {
            ShowError("Selected object did not have a skinned mesh renderer");
            return;
        }

        var mesh = mr.sharedMesh;

        var bytes = File.ReadAllBytes(path);
        var bs = VRCABlendShape.FromBytes(bytes);

        if (mesh.GetBlendShapeIndex(bs.Name) != -1)
        {
            ShowError($"BlendShape {bs.Name} already exists");
            return;
        }

        if (mesh_go.transform.localScale.x != bs.GetScale().x)
        {
            // bs.ToScale(mesh_go.transform.localScale);

            ShowError($"Your model scale ({mesh_go.transform.localScale.x}) differs from the original ({bs.GetScale().x}).\n"
                + "Please export it correctly or ask the creator for a version compatible with yours");
            return;
        }

        var modelMap = new ModelMapper(mesh);

        var vertMap = new int[bs.Verticies.Length];  // Map from VRCABlendShape indeces to Mesh indices
        for (var i = 0; i < bs.Verticies.Length; i++)
        {
            var index = modelMap.IdentifyVertex(bs.Verticies[i].Position, bs.Verticies[i].Triangle);

            if (index == -1)
                Debug.LogWarning($"Could not find vertex {bs.Verticies[i].Position}");

            vertMap[i] = index;
        }

        var deltaVerticies = new Vector3[mesh.vertexCount];
        var deltaNormals = new Vector3[mesh.vertexCount];
        var deltaTangents = new Vector3[mesh.vertexCount];

        for (int i = 0; i < vertMap.Length; i++)
        {
            // It was not mapped earlier
            if (vertMap[i] == -1)
                continue;

            deltaVerticies[vertMap[i]] = bs.VertDeltas[i];
            deltaNormals[vertMap[i]] = bs.VertDeltas[i];
            deltaTangents[vertMap[i]] = bs.VertDeltas[i];
        }

        mesh.AddBlendShapeFrame(bs.Name, 100.0f, deltaVerticies, deltaNormals, deltaTangents);
        //mesh.RecalculateNormals();
        //mesh.RecalculateTangents();
        //mesh.RecalculateBounds();

        //mesh.UploadMeshData(false);

        mr.SetBlendShapeWeight(mesh.GetBlendShapeIndex(bs.Name), 0);

        ClearTexts();
        ShowWarning("Done");
    }

    private void ClearTexts()
    {
        var error = rootVisualElement.Q<Label>("error");
        var warning = rootVisualElement.Q<Label>("warning");

        error.text = "";
        warning.text = "";
    }

    private void ShowError(string message)
    {
        var error = rootVisualElement.Q<Label>("error");
        error.text = message;
    }

    private void ShowWarning(string message)
    {
        var warning = rootVisualElement.Q<Label>("warning");
        warning.text = message;
    }
}
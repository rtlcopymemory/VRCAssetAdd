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
    }

    private void HandleMeshChange()
    {
        var out_mesh = rootVisualElement.Q<ObjectField>("out_mesh");
        var mesh_go = out_mesh.value as GameObject;

        var mr = mesh_go.GetComponent<SkinnedMeshRenderer>();

        if (mr == null)
        {
            // TODO: Add Error
            Debug.LogError("Selected object did not have a skinned mesh renderer");
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
        var popupField = rootVisualElement.Q<PopupField<string>>("shapes-popout");
        var blendshapeName = popupField.value;

        var out_mesh = rootVisualElement.Q<ObjectField>("out_mesh");
        var mesh_go = out_mesh.value as GameObject;

        var mr = mesh_go.GetComponent<SkinnedMeshRenderer>();

        if (mr == null)
        {
            // TODO: Add Error
            Debug.LogError("Selected object did not have a skinned mesh renderer");
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
            if (vertices[i].x != 0 || vertices[i].y != 0 || vertices[i].z != 0)
                affectedVerticies.Add(i);
        }

        var IdVerts = IdentifyVerticiesFromIndex(affectedVerticies, mesh);
        var vertDeltas = RemapDeltasArray(vertices, affectedVerticies);
        var normDeltas = RemapDeltasArray(normals, affectedVerticies);
        var tanDeltas = RemapDeltasArray(tangents, affectedVerticies);

        if (vertDeltas.Length != IdVerts.Count)
            throw new VRCAddException("Mismatches length of IDVerts and Deltas");

        var bsFile = new VRCABlendShape(blendshapeName, IdVerts.ToArray(), vertDeltas, normDeltas, tanDeltas);

        var path = EditorUtility.SaveFilePanel(
            "Save BlendShapeTransfer File",
            "",
            blendshapeName + "_BlendShape.bst",
            "bst");

        if (path.Length == 0)
        {
            Debug.LogError("Path was 0");
            return;
        }

        Debug.Log(bsFile.ConvertToBytes().Length);

        File.WriteAllBytes(path, bsFile.ConvertToBytes());
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
        foreach (var i in indeces)
        {
            var vertID = new VertexIdentifier
            {
                Position = mesh.vertices[i],
                Triangle = FindTriangleContaining(i, mesh)
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

    private VRCATriangle FindTriangleContaining(int indexToContain, Mesh mesh)
    {
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var tri = new VRCATriangle()
            {
                a = mesh.vertices[mesh.triangles[i]],
                b = mesh.vertices[mesh.triangles[i + 1]],
                c = mesh.vertices[mesh.triangles[i + 2]]
            };

            if (tri.Contains(mesh.vertices[indexToContain]))
                return tri;
        }

        return null;
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
}
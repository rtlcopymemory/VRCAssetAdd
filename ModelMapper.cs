using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VRCAssetAdd
{
    internal class ModelMapper
    {
        // List<int>: Because of how Unity has to split every polygon into tris
        // most if not all models will have overlapping vertices
        private readonly Dictionary<Vector3, List<int>> VertMapping;

        // This maps a position to all possible tris (first index of tri) it appears in
        // Speeds up vert recognition
        private readonly Dictionary<Vector3, List<int>> TrisMapping;

        ModelMapper(Mesh mesh)
        {
            VertMapping = new Dictionary<Vector3, List<int>>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (!VertMapping.ContainsKey(mesh.vertices[i]))
                    VertMapping[mesh.vertices[i]] = new List<int>();

                if (!TrisMapping.ContainsKey(mesh.vertices[i]))
                    TrisMapping[mesh.vertices[i]] = new List<int>();

                VertMapping[mesh.vertices[i]].Add(i);

                for (int j = 0; j < mesh.triangles.Length; j += 3)
                {
                    if (mesh.triangles[j] == i || mesh.triangles[j + 1] == i || mesh.triangles[j + 2] == i)
                    {
                        TrisMapping[mesh.vertices[i]].Add(j);
                    }
                }
            }
        }
    }
}

using Assets.VRCAssetAdd.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VRCAssetAdd
{
    public interface IModelMapper
    {
        int IdentifyVertex(Vector3 position, VRCATriangle triangle);
        VRCATriangle FindTriangleContaining(int index);
    }

    public class ModelMapper: IModelMapper
    {
        // List<int>: Because of how Unity has to split every polygon into tris
        // most if not all models will have overlapping vertices
        private readonly Dictionary<Vector3, List<int>> VertMapping;

        // This maps a position to all possible tris (first index of tri) it appears in
        // Speeds up vert recognition
        private readonly Dictionary<Vector3, List<int>> TrisMapping;

        // This maps the indices instead. Used for when I create the blendshape and
        // need to find a triangle that contains the index
        private readonly Dictionary<int, List<int>> TriIndexMapping;

        private readonly Mesh mesh;

        public ModelMapper(Mesh mesh)
        {
            this.mesh = mesh != null ? mesh : throw new ArgumentNullException();
            VertMapping = new Dictionary<Vector3, List<int>>();
            TrisMapping = new Dictionary<Vector3, List<int>>();
            TriIndexMapping = new Dictionary<int, List<int>>();

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (!VertMapping.ContainsKey(mesh.vertices[i]))
                    VertMapping[mesh.vertices[i]] = new List<int>();

                if (!TrisMapping.ContainsKey(mesh.vertices[i]))
                    TrisMapping[mesh.vertices[i]] = new List<int>();

                if (!TriIndexMapping.ContainsKey(i))
                    TriIndexMapping[i] = new List<int>();

                VertMapping[mesh.vertices[i]].Add(i);
            }

            for (int j = 0; j < mesh.triangles.Length; j += 3)
            {
                var i = mesh.triangles[j];
                TrisMapping[mesh.vertices[i]].Add(j); // Position to tri candidates
                TriIndexMapping[i].Add(j); // index to tri candidates

                i = mesh.triangles[j + 1];
                TrisMapping[mesh.vertices[i]].Add(j); // Position to tri candidates
                TriIndexMapping[i].Add(j); // index to tri candidates

                i = mesh.triangles[j + 2];
                TrisMapping[mesh.vertices[i]].Add(j); // Position to tri candidates
                TriIndexMapping[i].Add(j); // index to tri candidates
            }
        }

        public int IdentifyVertex(Vector3 position, VRCATriangle triangle)
        {
            if (!TrisMapping.ContainsKey(position))
            {
                throw new VRCAddException($"Could not find position ({position.x}, {position.y}, {position.z}) in the tris");
                // return -1;
            }

            var candidates = TrisMapping[position];
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var a = mesh.vertices[mesh.triangles[candidate]];
                var b = mesh.vertices[mesh.triangles[candidate + 1]];
                var c = mesh.vertices[mesh.triangles[candidate + 2]];
                if (triangle.Contains(a) && triangle.Contains(b) && triangle.Contains(c))
                {
                    return a == position ? mesh.triangles[candidate] : b == position ? mesh.triangles[candidate + 1] : mesh.triangles[candidate + 2];
                }
            }

            return -1;
        }

        public VRCATriangle FindTriangleContaining(int index)
        {
            var tri = TriIndexMapping[index].First();

            return new VRCATriangle()
            {
                a = mesh.vertices[mesh.triangles[tri]],
                b = mesh.vertices[mesh.triangles[tri + 1]],
                c = mesh.vertices[mesh.triangles[tri + 2]]
            };
        }
    }
}

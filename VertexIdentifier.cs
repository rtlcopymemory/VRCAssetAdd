using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VRCAssetAdd
{
    public class VertexIdentifier
    {
        public Vector3 Position { get; set; }
        public VRCATriangle Triangle { get; set; }
    }

    public class VRCATriangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public bool Contains(Vector3 vect)
        {
            return a == vect || b == vect || c == vect;
        }
    }
}

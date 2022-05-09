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
        /// TODO: These needs to be the coordinates of the verticies
        public int a;
        public int b;
        public int c;

        public bool Contains(int index)
        {
            return a == index || b == index || c == index;
        }
    }
}

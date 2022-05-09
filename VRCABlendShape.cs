using Assets.VRCAssetAdd.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VRCAssetAdd
{
    public class VRCABlendShape
    {
        private string Name;
        private VertexIdentifier[] Verticies;
        private Vector3[] VertDeltas;
        private Vector3[] NormDeltas;
        private Vector3[] TanDeltas;

        readonly private static string Signature = "BST1";

        public VRCABlendShape(string name, VertexIdentifier[] verticies, Vector3[] vertDeltas, Vector3[] normDeltas, Vector3[] tanDeltas)
        {
            Name = name;
            Verticies = verticies;
            VertDeltas = vertDeltas;
            NormDeltas = normDeltas;
            TanDeltas = tanDeltas;

            if (vertDeltas.Length != verticies.Length)
                throw new VRCAddException("vertDeltas has wrong size!");

            if (normDeltas.Length != verticies.Length)
                throw new VRCAddException("normDeltas has wrong size!");

            if (tanDeltas.Length != verticies.Length)
                throw new VRCAddException("tanDeltas has wrong size!");
        }

        public static VRCABlendShape FromBytes(byte[] bytes)
        {
            int index = 0;

            var sign = BitConverter.ToString(bytes, 0, Signature.Length);
            index += Signature.Length;
            if (sign != Signature)
            {
                throw new VRCAParsingException("Signature did not match");
            }

            var nameLen = BitConverter.ToInt32(bytes, index);
            index += 4;

            var name = BitConverter.ToString(bytes, index, nameLen);
            index += nameLen;

            List<VertexIdentifier> verticies = new List<VertexIdentifier>();
            List<Vector3> vertDeltas = new List<Vector3>();
            List<Vector3> normDeltas = new List<Vector3>();
            List<Vector3> tanDeltas = new List<Vector3>();
            while (index < bytes.Length)
            {
                var position = new Vector3
                {
                    x = BitConverter.ToInt32(bytes, index),
                    y = BitConverter.ToInt32(bytes, index + 4),
                    z = BitConverter.ToInt32(bytes, index + 8) 
                };
                index += 12;

                var triangle = new VRCATriangle()
                {
                    a = BitConverter.ToInt32(bytes, index),
                    b = BitConverter.ToInt32(bytes, index + 4),
                    c = BitConverter.ToInt32(bytes, index + 8),
                };
                index += 12;

                verticies.Add(new VertexIdentifier()
                {
                    Position = position,
                    Triangle = triangle
                });

                vertDeltas.Add(new Vector3()
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8)
                });
                index += 12;

                normDeltas.Add(new Vector3()
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8)
                });
                index += 12;

                tanDeltas.Add(new Vector3()
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8)
                });
                index += 12;
            }

            return new VRCABlendShape(name, verticies.ToArray(), vertDeltas.ToArray(), normDeltas.ToArray(), tanDeltas.ToArray());
        }

        public byte[] ConvertToBytes()
        {
            var bytes = new List<byte>();
            bytes.Concat(Encoding.ASCII.GetBytes(Signature));
            bytes.Concat(BitConverter.GetBytes(Name.Length));
            bytes.Concat(Encoding.ASCII.GetBytes(Name));

            for (int i = 0; i < Verticies.Length; i++)
            {
                bytes.Concat(BitConverter.GetBytes(Verticies[i].Position.x));
                bytes.Concat(BitConverter.GetBytes(Verticies[i].Position.y));
                bytes.Concat(BitConverter.GetBytes(Verticies[i].Position.z));

                bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.a));
                bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.b));
                bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.c));

                bytes.Concat(BitConverter.GetBytes(VertDeltas[i].x));
                bytes.Concat(BitConverter.GetBytes(VertDeltas[i].y));
                bytes.Concat(BitConverter.GetBytes(VertDeltas[i].z));

                bytes.Concat(BitConverter.GetBytes(NormDeltas[i].x));
                bytes.Concat(BitConverter.GetBytes(NormDeltas[i].y));
                bytes.Concat(BitConverter.GetBytes(NormDeltas[i].z));

                bytes.Concat(BitConverter.GetBytes(TanDeltas[i].x));
                bytes.Concat(BitConverter.GetBytes(TanDeltas[i].y));
                bytes.Concat(BitConverter.GetBytes(TanDeltas[i].z));
            }

            return bytes.ToArray();
        }
    }
}

// File specifications
// Signature: 42535431
// +-------------------------------------------------------------------------------+
// | 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 0A | 0B | 0C | 0D | 0E | 0F |
// +-------------------------------------------------------------------------------+
// |     Signature     |    Name Length    | Blendshape Name (variable size)       |
// +-------------------------------------------------------------------------------+
// |     Position.x    |     Position.y    |     Position.z    |     Triangle.a    |
// +-------------------------------------------------------------------------------+
// |     Triangle.b    |     Triangle.c    |    vertDeltas.x   |    vertDeltas.y   |
// +-------------------------------------------------------------------------------+
// |    vertDeltas.z   |    normDeltas.x   |    normDeltas.y   |    normDeltas.z   |
// +-------------------------------------------------------------------------------+
// |    tanDeltas.x    |    tanDeltas.y    |    tanDeltas.z    |        NEXT       |
// +-------------------------------------------------------------------------------+

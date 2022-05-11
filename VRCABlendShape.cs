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
        public string Name { get; }
        public VertexIdentifier[] Verticies { get; }
        public Vector3[] VertDeltas { get; }
        public Vector3[] NormDeltas { get; }
        public Vector3[] TanDeltas { get; }
        private Vector3 Scale;

        readonly private static string Signature = "BST1";

        public VRCABlendShape(string name, Vector3 scale, VertexIdentifier[] verticies, Vector3[] vertDeltas, Vector3[] normDeltas, Vector3[] tanDeltas)
        {
            Name = name;
            Verticies = verticies;
            VertDeltas = vertDeltas;
            NormDeltas = normDeltas;
            TanDeltas = tanDeltas;
            Scale = scale;

            if (scale.x != scale.y || scale.y != scale.z)
                throw new VRCAddException("Non uniform scale!");

            if (vertDeltas.Length != verticies.Length)
                throw new VRCAddException("vertDeltas has wrong size!");

            if (normDeltas.Length != verticies.Length)
                throw new VRCAddException("normDeltas has wrong size!");

            if (tanDeltas.Length != verticies.Length)
                throw new VRCAddException("tanDeltas has wrong size!");
        }

        public Vector3 GetScale()
        {
            return Scale;
        }

        public void ToScale(Vector3 scale)
        {
            if (scale.x != scale.y || scale.y != scale.z)
                throw new VRCAddException($"Non uniform scale! {scale}");

            var factor = (1 / Scale.x) * scale.x;

            for (int i = 0; i < Verticies.Length; i++)
            {
                Verticies[i].Position *= factor;
                Verticies[i].Triangle.a *= factor;
                Verticies[i].Triangle.b *= factor;
                Verticies[i].Triangle.c *= factor;

                VertDeltas[i] *= factor;
                NormDeltas[i] *= factor;
                TanDeltas[i] *= factor;
            }

            Scale = scale;
        }

        public static VRCABlendShape FromBytes(byte[] bytes)
        {
            int index = 0;

            var sign = Encoding.ASCII.GetString(bytes.Take(Signature.Length).ToArray());
            index += Signature.Length;
            if (sign != Signature)
            {
                throw new VRCAParsingException($"Signature did not match: {sign}");
            }

            var scale = new Vector3
            {
                x = BitConverter.ToSingle(bytes, index),
                y = BitConverter.ToSingle(bytes, index + 4),
                z = BitConverter.ToSingle(bytes, index + 8)
            };
            index += 12;

            var nameLen = BitConverter.ToInt32(bytes, index);
            index += 4;

            var name = Encoding.ASCII.GetString(bytes.Skip(index).Take(nameLen).ToArray());
            index += nameLen;

            List<VertexIdentifier> verticies = new List<VertexIdentifier>();
            List<Vector3> vertDeltas = new List<Vector3>();
            List<Vector3> normDeltas = new List<Vector3>();
            List<Vector3> tanDeltas = new List<Vector3>();
            while (index < bytes.Length)
            {
                var position = new Vector3
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8) 
                };
                index += 12;

                var a = new Vector3()
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8)
                };
                index += 12;

                var b = new Vector3()
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8)
                };
                index += 12;

                var c = new Vector3()
                {
                    x = BitConverter.ToSingle(bytes, index),
                    y = BitConverter.ToSingle(bytes, index + 4),
                    z = BitConverter.ToSingle(bytes, index + 8)
                };
                index += 12;

                var triangle = new VRCATriangle()
                {
                    a = a,
                    b = b,
                    c = c
                };

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

            return new VRCABlendShape(name, scale, verticies.ToArray(), vertDeltas.ToArray(), normDeltas.ToArray(), tanDeltas.ToArray());
        }

        public byte[] ConvertToBytes()
        {
            var bytes = new byte[0];
            bytes = bytes.Concat(Encoding.ASCII.GetBytes(Signature)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(Scale.x)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(Scale.y)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(Scale.z)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(Name.Length)).ToArray();
            bytes = bytes.Concat(Encoding.ASCII.GetBytes(Name)).ToArray();

            for (int i = 0; i < Verticies.Length; i++)
            {
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Position.x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Position.y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Position.z)).ToArray();

                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.a.x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.a.y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.a.z)).ToArray();

                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.b.x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.b.y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.b.z)).ToArray();

                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.c.x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.c.y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(Verticies[i].Triangle.c.z)).ToArray();

                bytes = bytes.Concat(BitConverter.GetBytes(VertDeltas[i].x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(VertDeltas[i].y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(VertDeltas[i].z)).ToArray();

                bytes = bytes.Concat(BitConverter.GetBytes(NormDeltas[i].x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(NormDeltas[i].y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(NormDeltas[i].z)).ToArray();

                bytes = bytes.Concat(BitConverter.GetBytes(TanDeltas[i].x)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(TanDeltas[i].y)).ToArray();
                bytes = bytes.Concat(BitConverter.GetBytes(TanDeltas[i].z)).ToArray();
            }

            return bytes;
        }
    }
}

// File specifications
// Signature: 42535431
// +-------------------------------------------------------------------------------+
// | 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 0A | 0B | 0C | 0D | 0E | 0F |
// +-------------------------------------------------------------------------------+
// |     Signature     |      Scale.x      |      Scale.y      |      Scale.z      |
// +-------------------------------------------------------------------------------+
// |    Name Length    |              Blendshape Name (variable size)              |
// +-------------------------------------------------------------------------------+
// |     Position.x    |     Position.y    |     Position.z    |    Triangle.a.x   |
// +-------------------------------------------------------------------------------+
// |    Triangle.a.y   |    Triangle.a.z   |    Triangle.b.x   |    Triangle.b.y   |  
// +-------------------------------------------------------------------------------+
// |    Triangle.b.z   |    Triangle.c.x   |    Triangle.c.y   |    Triangle.c.z   |  
// +-------------------------------------------------------------------------------+
// |    vertDeltas.x   |    vertDeltas.y   |    vertDeltas.z   |    normDeltas.x   |
// +-------------------------------------------------------------------------------+
// |    normDeltas.y   |    normDeltas.z   |    tanDeltas.x    |    tanDeltas.y    |
// +-------------------------------------------------------------------------------+
// |    tanDeltas.z    |        NEXT                                               |
// +-------------------------------------------------------------------------------+

using FbxSharp;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Mesh
{
    public static class MeshProcessor
    {

        public static string GetHelp()
        {
            return "";
        }



        public static uint CompressNormal(float x, float y, float z)
        {
            //Convert cartesian to spherical
            double theta = System.Math.Atan2(y, x) * 180.0d / System.Math.PI;
            double phi = System.Math.Acos(z) * 180.0d / System.Math.PI;

            unchecked
            {
                const int scale = 100;

                uint theta_us = (ushort)(theta * scale);
                uint phi_us = (ushort)(phi * scale);

                return theta_us | (phi_us << 16);
            }
        }

        /*
        public static uint CompressNormal(float x, float y, float z)
        {
            uint x_i = (uint)(System.Math.Abs(x) * ((1 << 14) - 1));
            uint y_i = (uint)(System.Math.Abs(y) * 1023);
            uint z_i = (uint)(System.Math.Abs(z) * ((1 << 14) - 1));

            uint result = 0;

            result |= (x_i & 1023);
            result |= (uint)((y >= 0 ? 0 : 2) << 10);
            result |= (z_i >> 10) << 12;
            result |= (x_i >> 10) << 16;
            result |= (z_i & 1023) << 20;
            result |= (uint)(((z >= 0 ? 0 : 2) | (x >= 0 ? 0 : 1)) /*Allows application of the sign in a shader using (1 - z)*/// << 30);


        //   return result;
        //   return (uint)((x_i & 1023) | ((y >= 0 ? 0 : 2) << 10) | ((z_i & 1023) << 20) | (((z >= 0 ? 0 : 2) | (x >= 0 ? 0 : 1)) /*Allows application of the sign in a shader using (1 - z)*/ << 30));
        // }

        private static void ProcessMesh(FbxSharp.Mesh m, bool isStatic, string outputFile)
        {
            var Vertices = m.GetControlPoints();

            int texCoordLayer = 0;
            int normalLayer = 0;

            for (int i = 0; i < m.GetLayerCount(); i++)
            {
                var Layer = m.GetLayer(i);
                if (Layer.GetUVs() != null)
                    texCoordLayer = i;

                if (Layer.GetNormals() != null)
                    normalLayer = i;

            }
            var TexCoords = m.GetLayer(texCoordLayer).GetUVs();
            var Norms = m.GetLayer(texCoordLayer).GetNormals();

            List<float> vertices = new List<float>();
            List<float> uvs = new List<float>();
            List<float> normals = new List<float>();
            List<ushort> indices = new List<ushort>();


            for (int i = 0; i < Vertices.Length; i++)
            {
                vertices.Add((float)Vertices[i].X);
                vertices.Add((float)Vertices[i].Y);
                vertices.Add((float)Vertices[i].Z);
            }


            if (TexCoords.GetMappingMode() != LayerElement.EMappingMode.ByControlPoint)
                throw new Exception("UV Data unreadable.");

            var TC = TexCoords.GetDirectArray();
            var IndexData = TexCoords.GetIndexArray();
            var RefMode = TexCoords.GetReferenceMode();

            for (int i = 0; i < Vertices.Length; i++)
            {
                if (RefMode == LayerElement.EReferenceMode.Direct)
                {
                    uvs.Add((float)TC[i].X);
                    uvs.Add((float)TC[i].Y);
                }
                else if (RefMode == LayerElement.EReferenceMode.Index)
                {
                    uvs.Add((float)TC[IndexData[i]].X);
                    uvs.Add((float)TC[IndexData[i]].Y);
                }
            }

            var NC = Norms.GetDirectArray();
            IndexData = Norms.GetIndexArray();
            RefMode = Norms.GetReferenceMode();

            for (int i = 0; i < Vertices.Length; i++)
            {
                if (RefMode == LayerElement.EReferenceMode.Direct)
                {
                    normals.Add((float)NC[i].X);
                    normals.Add((float)NC[i].Y);
                    normals.Add((float)NC[i].Z);
                }
                else if (RefMode == LayerElement.EReferenceMode.Index)
                {
                    normals.Add((float)NC[IndexData[i]].X);
                    normals.Add((float)NC[IndexData[i]].Y);
                    normals.Add((float)NC[IndexData[i]].Z);
                }
            }

            int[] indices_d = null;
            if (indices_d == null)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    indices.Add((ushort)i);
                }
            }
            else
            {
                for (int i = 0; i < indices_d.Length; i++)
                {
                    indices.Add((ushort)indices_d[i]);
                }
            }


            //Compress the data
            List<float> c_vertices = new List<float>();
            List<ushort> c_uvs = new List<ushort>();
            List<uint> c_normals = new List<uint>();
            List<ushort> c_indices = new List<ushort>();

            for (int i = 0; i < vertices.Count; i += 3)
            {
                c_vertices.Add(vertices[i]);
                c_vertices.Add(vertices[i + 1]);
                c_vertices.Add(vertices[i + 2]);
            }

            for (int i = 0; i < uvs.Count; i += 2)
            {
                c_uvs.Add((ushort)(uvs[i] * ushort.MaxValue));
                c_uvs.Add((ushort)(uvs[i + 1] * ushort.MaxValue));
            }

            for (int i = 0; i < normals.Count; i += 3)
            {
                c_normals.Add(CompressNormal(normals[i], normals[i + 1], normals[i + 2]));
            }

            for (int i = 0; i < indices.Count; i++)
            {
                c_indices.Add(indices[i]);
            }

            //use the center of each triangle as a sample point, storing the triangle position and normal
            if (isStatic)
            {
                List<float> gi_pos = new List<float>();
                List<float> gi_norm = new List<float>();

                for (int i = 0; i < indices.Count; i += 3)
                {
                    var i_0 = indices[i];
                    var i_1 = indices[i + 1];
                    var i_2 = indices[i + 2];

                    gi_pos.Add((vertices[i_0 * 3] + vertices[i_1 * 3] + vertices[i_2 * 3]) / 3);
                    gi_pos.Add((vertices[i_0 * 3 + 1] + vertices[i_1 * 3 + 1] + vertices[i_2 * 3 + 1]) / 3);
                    gi_pos.Add((vertices[i_0 * 3 + 2] + vertices[i_1 * 3 + 2] + vertices[i_2 * 3 + 2]) / 3);

                    gi_norm.Add((normals[i_0 * 3] + normals[i_1 * 3] + normals[i_2 * 3]) / 3);
                    gi_norm.Add((normals[i_0 * 3 + 1] + normals[i_1 * 3 + 1] + normals[i_2 * 3 + 1]) / 3);
                    gi_norm.Add((normals[i_0 * 3 + 2] + normals[i_1 * 3 + 2] + normals[i_2 * 3 + 2]) / 3);
                }

                using (FileStream oFile = File.Open(Path.ChangeExtension(outputFile, "gi_base"), FileMode.Create))
                using (BinaryWriter w = new BinaryWriter(oFile))
                {
                    w.Write(('G' | 'I' << 8 | 'B' << 16 | 'A' << 24));
                    w.Write(gi_pos.Count * sizeof(float));
                    w.Write(System.Math.Max(m.BBoxMax.Value.X - m.BBoxMin.Value.X, System.Math.Max(m.BBoxMax.Value.Y - m.BBoxMin.Value.Y, m.BBoxMax.Value.Z - m.BBoxMin.Value.Z)));
                    w.Write(m.BBoxMin.Value.X);
                    w.Write(m.BBoxMin.Value.Y);
                    w.Write(m.BBoxMin.Value.Z);

                    for (int i = 0; i < gi_pos.Count; i++)
                    {
                        w.Write(gi_pos[i]);
                    }

                    for (int i = 0; i < gi_norm.Count; i++)
                    {
                        w.Write(gi_norm[i]);
                    }

                    w.Flush();
                }
            }

            //Write the compressed data to the file
            if (isStatic)
            {
                //TODO optimize all meshes for vertex cache performance and reduced overdraw using AMD's Tootle - emit an obj file, then read that data back in

                uint indexOffset = 6 * sizeof(uint);
                uint vertexOffset = (uint)(indexOffset + c_indices.Count * sizeof(ushort));
                uint uvOffset = (uint)(vertexOffset + c_vertices.Count * sizeof(float));
                uint normalOffset = (uint)(uvOffset + c_uvs.Count * sizeof(ushort));
                uint normalLen = (uint)(c_normals.Count * sizeof(uint));

                using (FileStream oFile = File.Open(outputFile, FileMode.Create))
                using (BinaryWriter w = new BinaryWriter(oFile))
                {
                    w.Write(('M' | 'E' << 8 | 'S' << 16 | 'H' << 24));
                    w.Write(indexOffset);
                    w.Write(vertexOffset);
                    w.Write(uvOffset);
                    w.Write(normalOffset);
                    w.Write(normalLen);

                    for (int i = 0; i < c_indices.Count; i++)
                    {
                        w.Write(c_indices[i]);
                    }

                    for (int i = 0; i < c_vertices.Count; i++)
                    {
                        w.Write(c_vertices[i]);
                    }

                    for (int i = 0; i < c_uvs.Count; i++)
                    {
                        w.Write(c_uvs[i]);
                    }

                    for (int i = 0; i < c_normals.Count; i++)
                    {
                        w.Write(c_normals[i]);
                    }

                    w.Flush();
                }
            }
        }

        private static void ProcessNode(Node r, bool isStatic, string outputFile)
        {
            for (int i = 0; i < r.ChildNodes.Count; i++)
            {
                Node n = r.ChildNodes[i];
                NodeAttribute attr = n.GetNodeAttribute();

                switch (attr.AttributeType)
                {
                    case NodeAttribute.EAttributeType.Mesh:
                        {
                            ProcessMesh((FbxSharp.Mesh)attr, isStatic, outputFile);
                        }
                        break;
                    case NodeAttribute.EAttributeType.Light:
                        {
                            Light l = (Light)attr;
                            //l.
                        }
                        break;
                    case NodeAttribute.EAttributeType.Camera:
                        {
                            Camera c = (Camera)attr;
                        }
                        break;
                }

            }
        }

        public static void Preprocess(string inputFile, float scale, string outputFile)
        {
            bool isStatic = true;

            if (scale == 0) scale = 1;
            if (inputFile == "") throw new Exception();
            if (outputFile == "") outputFile = Path.ChangeExtension(inputFile, isStatic ? "k4_stmesh" : "k4_dymesh");

            Importer imp = new Importer();
            Console.WriteLine($"{inputFile}");
            Scene scene = imp.Import(inputFile);

            ProcessNode(scene.RootNode, isStatic, outputFile);

            //Convert the data to a format we can work with easily

        }
    }
}

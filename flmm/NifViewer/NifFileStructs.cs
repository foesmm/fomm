using System;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace fomm.NifViewer {
    partial class NifFile {
        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private static extern LoadReturn Load(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1)] byte[] data, int size);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private static extern bool GetMaterial(uint ID, out Material mat);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private unsafe static extern bool GetVerticies(uint ID, ConvertedVertex* vert);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private static extern bool GetTransform(uint ID, out Matrix mat);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private static extern bool GetInfo(uint ID, out SubsetInfo mat);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private unsafe static extern bool GetTex(uint ID, out sbyte* tex);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private unsafe static extern bool GetIndicies(uint ID, ushort* strip);

        [DllImport("NifScanner.dll", CharSet=CharSet.Ansi)]
        private static extern void ResaveNif(string outpath, 
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1)] byte[] parallaxed);

        private unsafe struct LoadReturn {
            public uint Subsets;
            public int FailedSubsets;
            public float maxsize;
            public sbyte* log;
        }

        public struct SubsetData {
            public string path;
            public Format cformat;
            public Format nformat;
            public Format gformat;
            public int cWidth;
            public int cHeight;
            public int nWidth;
            public int nHeight;
            public int gWidth;
            public int gHeight;
        }

        private struct SubsetInfo {
            public Vector3 Center;
            public float Radius;
            public float alpha;
            public int vCount;
            public int iCount;
            [MarshalAs(UnmanagedType.I1)]
            public bool containsTexCoords;
            [MarshalAs(UnmanagedType.I1)]
            public bool containsNormals;
            [MarshalAs(UnmanagedType.I1)]
            public bool containsColor;
            [MarshalAs(UnmanagedType.I1)]
            public bool containsTangentBinormal;
            [MarshalAs(UnmanagedType.I1)]
            public bool containsTexture;
            [MarshalAs(UnmanagedType.I1)]
            public bool containsIndicies;
            [MarshalAs(UnmanagedType.I1)]
            public byte applyMode;
            [MarshalAs(UnmanagedType.I1)]
            public bool wireframe;
            [MarshalAs(UnmanagedType.I1)]
            public bool hasalpha;
        }

        private struct Subset {
            public SubsetData data;
            public SubsetInfo info;
            public Material mat;
            public Matrix transform;
            //public TexInfo texInfo;

            public VertexDeclaration vDecl;
            public VertexBuffer vBuffer;
            public IndexBuffer iBuffer;
            public int numTris;
            public Texture colorMap;
            public Texture normalMap;
            public Texture glowMap;
            public Texture oldColorMap;
        }

        private struct ConvertedVertex {
            public Vector3 Position;
            public Vector3 Tangent;
            public Vector3 Binormal;
            public Vector3 Normal;
            public Vector2 TexCoords;
            public Vector4 Color;
        }
    }
}
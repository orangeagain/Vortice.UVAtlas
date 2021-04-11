// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vortice.UVAtlas
{
    public static class UVAtlas
    {
        public static Result Create(
            Vector3[] positions, int verticesCount,
            ushort[] indices, int facesCount,
            uint[] adjacency,
            int maxChartNumber = 0, float maxStretch = 0.16667f, float gutter = 2.0f,
            int width = 512, int height = 512,
            float callbackFrequency = 0.0001f,
            Options options = Options.Default)
        {
            Result result = new Result();

            unsafe
            {
                fixed (Vector3* positionsPtr = &positions[0])
                {
                    fixed (ushort* indicesPtr = &indices[0])
                    {
                        fixed (uint* adjacencyPtr = &adjacency[0])
                        {
                            UVAtlasResult* uv_result = UVAtlasCreate(
                                positionsPtr, verticesCount,
                                indicesPtr, false, facesCount,
                                maxChartNumber, maxStretch,
                                width, height, gutter,
                                adjacencyPtr,
                                null,
                                null,
                                callbackFrequency,
                                options, out int returnCode);

                            if (returnCode < 0)
                            {
                                result.ResultCode = returnCode;
                                return result;
                            }

                            result.VerticesCount = uv_result->VerticesCount;
                            result.Vertices = new Vertex[uv_result->VerticesCount];
                        }
                    }
                }
            }

            return result;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe readonly struct UVAtlasResult
        {
            public readonly uint VerticesCount;
            public readonly Vertex* Vertices;
            public readonly uint IndicesCount;
            public readonly byte* Indices;
            public readonly float Stretch;
            public readonly nint Charts;
        }

        [DllImport("UVAtlas.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "uvatlas_create")]
        private unsafe static extern UVAtlasResult* UVAtlasCreate(
            Vector3* positions, nint nVerts,
            void* indices, bool is32Bit, nint nFaces,
            nint maxChartNumber, float maxStretch,
            nint width, nint height,
            float gutter,
            uint* adjacency, uint* falseEdgeAdjacency,
            float* pIMTArray,
            float callbackFrequency,
            Options options,
            out int returnCode);
    }

    [Flags]
    public enum Options
    {
        Default = 0x00,
        GeodesicFast = 0x01,
        GeodesicQuality = 0x02,
        LimitMergeStretch = 0x04,
        LimitFaceStretch = 0x08,
    }

    public readonly struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector2 TexCoord;

        public Vertex(in Vector3 position, in Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
    }

    public struct Result
    {
        public int ResultCode;

        public uint VerticesCount;
        public Vertex[] Vertices;
        public uint IndicesCount;
        public ushort[] Indices;
        public float Stretch;
        public nint Charts;


        public bool IsFailure => ResultCode < 0;
    }
}

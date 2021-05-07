// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vortice.UVAtlas
{
    public static class UVAtlas
    {
        public static Result Create(
            Vector3[] positions,
            uint[] indices,
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
                    fixed (uint* indicesPtr = &indices[0])
                    {
                        fixed (uint* adjacencyPtr = &adjacency[0])
                        {
                            Native.UVAtlasResult* uv_result = UVAtlasCreate(
                                positionsPtr, positions.Length,
                                indicesPtr, indices.Length / 3,
                                maxChartNumber, maxStretch,
                                width, height, gutter,
                                adjacencyPtr,
                                null,
                                null,
                                callbackFrequency,
                                options);

                            if (uv_result->Result < 0)
                            {
                                result.ResultCode = uv_result->Result;
                                return result;
                            }

                            result.VerticesCount = uv_result->VerticesCount;
                            result.Vertices = new Vertex[uv_result->VerticesCount];
                            result.IndicesCount = uv_result->IndicesCount;
                            result.Indices = new uint[uv_result->IndicesCount];
                            result.FacePartitioning = new uint[uv_result->VerticesCount];
                            result.VertexRemapArray = new uint[uv_result->VerticesCount];
                            result.Stretch = uv_result->Stretch;
                            result.Charts = uv_result->Charts;
                            Write(result.Vertices, uv_result->Vertices, uv_result->VerticesCount);
                            Write(result.Indices, uv_result->Indices, uv_result->IndicesCount);
                            Write(result.FacePartitioning, uv_result->FacePartitioning, uv_result->VerticesCount);
                            Write(result.VertexRemapArray, uv_result->VertexRemapArray, uv_result->VerticesCount);
                            Native.uvatlas_delete(uv_result);
                        }
                    }
                }
            }

            return result;
        }

        public static unsafe void Write<T>(T[] destination, void* data, uint count) where T : unmanaged
        {
            fixed (void* destinationPtr = destination)
            {
                Unsafe.CopyBlockUnaligned(destinationPtr, data, (uint)(count * sizeof(T)));
            }
        }


        [DllImport("UVAtlas.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "uvatlas_create_uint32")]
        private unsafe static extern Native.UVAtlasResult* UVAtlasCreate(
            Vector3* positions, nint nVerts,
            uint* indices, nint nFaces,
            nint maxChartNumber, float maxStretch,
            nint width, nint height,
            float gutter,
            uint* adjacency, uint* falseEdgeAdjacency,
            float* pIMTArray,
            float callbackFrequency,
            Options options);
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

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(TexCoord)}: {TexCoord}";
        }
    }

    public struct Result
    {
        public int ResultCode;

        public uint VerticesCount;
        public Vertex[] Vertices;
        public uint IndicesCount;
        public uint[] Indices;
        public uint[] FacePartitioning;
        public uint[] VertexRemapArray;
        public float Stretch;
        public uint Charts;

        public bool IsFailure => ResultCode < 0;
    }
}

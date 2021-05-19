using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Vortice.UVAtlas
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }
    public struct Vector2
    {
        public float x;
        public float y;
    }
    public static class VorticeUVAtlasWrap
    {
        public struct UvResult
        {
            public Int32 ResultCode;

            public uint VerticesCount;
            public Vertex[] Vertices;
            public uint IndicesCount;
            public uint[] Indices;
            public uint[] FacePartitioning;
            public uint[] VertexRemapArray;
            public float Stretch;
            public nint Charts;

            public bool IsFailure => ResultCode < 0;
        }

        public const string UVAtlasDll = "UVAtlas";

        [DllImport(UVAtlasDll, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern Int32 GenerateAdjacencyAndPointReps(
            UInt32* indices, nint nFaces,
            Vector3* positions, nint nVerts,
            float epsilon,
            UInt32* pointRep,
            UInt32* adjacency
        );
        public static unsafe bool GenerateAdjacencyAndPointReps_Wrap(
                uint[] indices,
                Vector3[] positions, out uint[] adj,
                float epsilon = 0.0f)
        {
            fixed (uint* indicesPtr = indices)
            {
                fixed (Vector3* positionsPtr = positions)
                {
                    adj = new uint[indices.Length];
                    fixed (uint* adjPtr = adj)
                    {
                        Int32 code = GenerateAdjacencyAndPointReps(
                            indicesPtr, indices.Length / 3,
                            positionsPtr, positions.Length,
                            epsilon,
                            null,
                            adjPtr);


                        if (code < 0)
                        {
                            Console.WriteLine("ComputeAdjErrorCode:" + code);
                            return false;
                        }
                        else
                            return true;
                    }
                }
            }
        }

        public enum VALIDATE_FLAGS : ulong
        {
            VALIDATE_DEFAULT = 0x0,
            VALIDATE_BACKFACING = 0x1,// Check for duplicate neighbor from triangle (requires adjacency)
            VALIDATE_BOWTIES = 0x2,// Check for two fans of triangles using the same vertex (requires adjacency)
            VALIDATE_DEGENERATE = 0x4,// Check for degenerate triangles
            VALIDATE_UNUSED = 0x8,// Check for issues with 'unused' triangles
            VALIDATE_ASYMMETRIC_ADJ = 0x10,// Checks that neighbors are symmetric (requires adjacency)
        };

        [DllImport(UVAtlasDll, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int Validate(
            UInt32* indices, nint nFaces,
            nint nVerts, UInt32* adjacency,
            VALIDATE_FLAGS flags, IntPtr* msgs
            );

        public static unsafe bool Validate_Wrap(
               uint[] indices,
               UInt32[] adjacency,
               VALIDATE_FLAGS flags
               )
        {
            fixed (uint* indicesPtr = indices)
            {
                fixed (UInt32* adjacencyPtr = adjacency)
                {
                    int result = Validate(
                        indicesPtr, indices.Length / 3,
                        indices.Length / 3,
                        adjacencyPtr,
                        flags, null);

                    if (result >= 0)
                        return true;
                    else
                    {
                        Console.WriteLine("Validate_Wrap:result:" + result);
                        return false;
                    }

                }
            }
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



        [Flags]
        public enum Options
        {
            Default = 0x00,
            GeodesicFast = 0x01,
            GeodesicQuality = 0x02,
            LimitMergeStretch = 0x04,
            LimitFaceStretch = 0x08,
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe readonly struct UvResultsNative
        {
            public readonly Int32 Code;
            public readonly uint VerticesCount;
            public readonly uint IndicesCount;
            public readonly Vertex* Vertices;
            public readonly uint* Indices;
            public readonly uint* FacePartitioning;
            public readonly uint* VertexRemapArray;
            public readonly float Stretch;
            public readonly nint Charts;
        }

        public static bool Create(
                Vector3[] positions,
                uint[] indices,
                uint[] adjacency, out UvResult uvResult,
                int maxChartNumber = 0, float maxStretch = 0.16667f, float gutter = 2.0f,
                int width = 512, int height = 512,
                float callbackFrequency = 0.1f,
                Options options = Options.Default)
        {
            uvResult = new UvResult();

            unsafe
            {
                fixed (Vector3* positionsPtr = &positions[0])
                {
                    fixed (uint* indicesPtr = &indices[0])
                    {
                        fixed (uint* adjacencyPtr = &adjacency[0])
                        {
                            UvResultsNative* uvResultsNative = ComputeUV(
                                positionsPtr, positions.Length,
                                indicesPtr, indices.Length / 3,
                                maxChartNumber, maxStretch,
                                width, height, gutter,
                                adjacencyPtr,
                                null,
                                null,
                                callbackFrequency,
                                options);

                            uvResult.ResultCode = uvResultsNative->Code;

                            if (uvResultsNative->Code < 0)
                                return false;

                            uvResult.VerticesCount = uvResultsNative->VerticesCount;
                            uvResult.Vertices = new Vertex[uvResultsNative->VerticesCount];
                            uvResult.IndicesCount = uvResultsNative->IndicesCount;
                            uvResult.Indices = new uint[uvResultsNative->IndicesCount];
                            uvResult.FacePartitioning = new uint[uvResultsNative->VerticesCount];
                            uvResult.VertexRemapArray = new uint[uvResultsNative->VerticesCount];
                            uvResult.Stretch = uvResultsNative->Stretch;
                            uvResult.Charts = uvResultsNative->Charts;
                            Write(uvResult.Vertices, uvResultsNative->Vertices, uvResultsNative->VerticesCount);
                            Write(uvResult.Indices, uvResultsNative->Indices, uvResultsNative->IndicesCount);
                            Write(uvResult.FacePartitioning, uvResultsNative->FacePartitioning, uvResultsNative->VerticesCount);
                            Write(uvResult.VertexRemapArray, uvResultsNative->VertexRemapArray, uvResultsNative->VerticesCount);
                            uvatlas_delete(uvResultsNative);
                            return true;
                        }
                    }
                }
            }
        }

        [DllImport(UVAtlasDll, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern UvResultsNative* ComputeUV(
                Vector3* positions, nint nVerts,
                uint* indices, nint nFaces,
                nint maxChartNumber, float maxStretch,
                nint width, nint height,
                float gutter,
                uint* adjacency, uint* falseEdgeAdjacency,
                float* pIMTArray,
                float callbackFrequency,
                Options options);

        [DllImport(UVAtlasDll, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern void uvatlas_delete(UvResultsNative* result);

        public static unsafe void Write<T>(T[] destination, void* data, uint desCount) where T : unmanaged
        {
            fixed (void* destinationPtr = destination)
            {
                long bytesCount = desCount * sizeof(T);
                Buffer.MemoryCopy(data, destinationPtr, bytesCount, bytesCount);
            }
        }

    }
}

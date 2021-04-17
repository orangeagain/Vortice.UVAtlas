// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Vortice.UVAtlas
{
    internal unsafe static class Native
    {
        #region Native callback and structs
        private static readonly IntPtr s_NativeLibrary;
        private static T LoadFunction<T>(string name) => LibraryLoader.LoadFunction<T>(s_NativeLibrary, name);

        static Native()
        {
            s_NativeLibrary = LibraryLoader.LoadLocalLibrary("UVAtlas.dll");
            s_GenerateAdjacencyAndPointReps_UInt16 = LoadFunction<GenerateAdjacencyAndPointReps_UInt16_t>("GenerateAdjacencyAndPointReps_UInt16");
            s_GenerateAdjacencyAndPointReps_UInt32 = LoadFunction<GenerateAdjacencyAndPointReps_UInt32_t>("GenerateAdjacencyAndPointReps_UInt32");
            s_ComputeNormals_UInt16 = LoadFunction<ComputeNormals_UInt16_t>(nameof(ComputeNormals_UInt16));
            s_ComputeNormals_UInt32 = LoadFunction<ComputeNormals_UInt32_t>(nameof(ComputeNormals_UInt32));

            uvatlas_delete = LoadFunction<uvatlas_delete_t>("uvatlas_delete");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GenerateAdjacencyAndPointReps_UInt16_t(ushort* indices, nint nFaces, Vector3* positions, nint nVerts, float epsilon, uint* pointRep, uint* adjacency);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GenerateAdjacencyAndPointReps_UInt32_t(uint* indices, nint nFaces, Vector3* positions, nint nVerts, float epsilon, uint* pointRep, uint* adjacency);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ComputeNormals_UInt16_t(ushort* indices, nint nFaces, Vector3* positions, nint nVerts, ComputeNormalsFlags flags, Vector3* normals);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ComputeNormals_UInt32_t(uint* indices, nint nFaces, Vector3* positions, nint nVerts, ComputeNormalsFlags flags, Vector3* normals);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uvatlas_delete_t(UVAtlasResult* result);

        private static readonly GenerateAdjacencyAndPointReps_UInt16_t s_GenerateAdjacencyAndPointReps_UInt16;
        private static readonly GenerateAdjacencyAndPointReps_UInt32_t s_GenerateAdjacencyAndPointReps_UInt32;
        private static readonly ComputeNormals_UInt16_t s_ComputeNormals_UInt16;
        private static readonly ComputeNormals_UInt32_t s_ComputeNormals_UInt32;

        public static readonly uvatlas_delete_t uvatlas_delete;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe readonly struct UVAtlasResult
        {
            public readonly int Result;
            public readonly uint VerticesCount;
            public readonly uint IndicesCount;
            public readonly Vertex* Vertices;
            public readonly uint* Indices;
            public readonly uint* FacePartitioning;
            public readonly uint* VertexRemapArray;
            public readonly float Stretch;
            public readonly uint Charts;
        }
        #endregion

        public static int GenerateAdjacencyAndPointReps_UInt16(
            ushort* indices, nint nFaces,
            Vector3* positions, nint nVerts,
            float epsilon,
            uint* pointRep, uint* adjacency)
        {
            return s_GenerateAdjacencyAndPointReps_UInt16(indices, nFaces, positions, nVerts, epsilon, pointRep, adjacency);
        }

        public static int GenerateAdjacencyAndPointReps_UInt32(
            uint* indices, nint nFaces,
            Vector3* positions, nint nVerts,
            float epsilon,
            uint* pointRep, uint* adjacency)
        {
            return s_GenerateAdjacencyAndPointReps_UInt32(indices, nFaces, positions, nVerts, epsilon, pointRep, adjacency);
        }

        public static int ComputeNormals_UInt16(
            ushort* indices, nint nFaces,
            Vector3* positions, nint nVerts,
            ComputeNormalsFlags flags,
            Vector3* normals)
        {
            return s_ComputeNormals_UInt16(indices, nFaces, positions, nVerts, flags, normals);
        }

        public static int ComputeNormals_UInt32(
            uint* indices, nint nFaces,
            Vector3* positions, nint nVerts,
            ComputeNormalsFlags flags,
            Vector3* normals)
        {
            return s_ComputeNormals_UInt32(indices, nFaces, positions, nVerts, flags, normals);
        }


    }
}

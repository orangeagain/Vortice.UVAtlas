// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Numerics;

namespace Vortice.UVAtlas
{
    public static class MeshUtilities
    {
        public static unsafe uint[] GenerateAdjacencyAndPointReps(
            Span<ushort> indices,
            Span<Vector3> positions,
            float epsilon = 0.0f)
        {
            fixed (ushort* indicesPtr = indices)
            {
                fixed (Vector3* positionsPtr = positions)
                {
                    uint[] adj = new uint[indices.Length];
                    fixed (uint* adjPtr = adj)
                    {
                        int result = Native.GenerateAdjacencyAndPointReps_UInt16(
                            indicesPtr,
                            indices.Length / 3,
                            positionsPtr,
                            positions.Length,
                            epsilon,
                            null,
                            adjPtr);

                        if (result < 0)
                            return default;

                        return adj;
                    }
                }
            }
        }

        public static unsafe uint[] GenerateAdjacencyAndPointReps(
            Span<ushort> indices,
            Span<Vector3> positions,
            out uint[] pointRep,
            float epsilon = 0.0f)
        {
            fixed (ushort* indicesPtr = indices)
            {
                fixed (Vector3* positionsPtr = positions)
                {
                    uint[] adj = new uint[indices.Length];
                    fixed (uint* adjPtr = adj)
                    {
                        fixed (uint* pointRepPtr = pointRep)
                        {
                            int result = Native.GenerateAdjacencyAndPointReps_UInt16(
                                indicesPtr,
                                indices.Length / 3,
                                positionsPtr,
                                positions.Length,
                                epsilon,
                                pointRepPtr,
                                adjPtr);

                            if (result < 0)
                                return default;

                            return adj;
                        }
                    }
                }
            }
        }

        public static unsafe uint[] GenerateAdjacencyAndPointReps(
            Span<uint> indices,
            Span<Vector3> positions,
            float epsilon = 0.0f)
        {
            fixed (uint* indicesPtr = indices)
            {
                fixed (Vector3* positionsPtr = positions)
                {
                    uint[] adj = new uint[indices.Length];
                    fixed (uint* adjPtr = adj)
                    {
                        int result = Native.GenerateAdjacencyAndPointReps_UInt32(
                            indicesPtr,
                            indices.Length / 3,
                            positionsPtr,
                            positions.Length,
                            epsilon,
                            null,
                            adjPtr);

                        if (result < 0)
                            return default;

                        return adj;
                    }
                }
            }
        }

        public static unsafe uint[] GenerateAdjacencyAndPointReps(
            Span<uint> indices,
            Span<Vector3> positions,
            out uint[] pointRep,
            float epsilon = 0.0f)
        {
            fixed (uint* indicesPtr = indices)
            {
                fixed (Vector3* positionsPtr = positions)
                {
                    uint[] adj = new uint[indices.Length];
                    fixed (uint* adjPtr = adj)
                    {
                        fixed (uint* pointRepPtr = pointRep)
                        {
                            int result = Native.GenerateAdjacencyAndPointReps_UInt32(
                                indicesPtr,
                                indices.Length / 3,
                                positionsPtr,
                                positions.Length,
                                epsilon,
                                pointRepPtr,
                                adjPtr);

                            if (result < 0)
                                return default;

                            return adj;
                        }
                    }
                }
            }
        }
    }

    [Flags]
    public enum ComputeNormalsFlags : uint
    {
        /// <summary>
        /// Default is to compute normals using weight-by-angle
        /// </summary>
        Default = 0,
        /// <summary>
        /// Computes normals using weight-by-area
        /// </summary>
        WeightByArea = 0x1,
        /// <summary>
        /// Compute normals with equal weights
        /// </summary>
        WeightEqual = 0x2,
        /// <summary>
        /// Vertices are clock-wise (defaults to CCW)
        /// </summary>
        WindClockwise = 0x4,
    }

}

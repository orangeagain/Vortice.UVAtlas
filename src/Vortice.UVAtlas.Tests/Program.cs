using System;
using System.Numerics;

namespace Vortice.UVAtlas
{
    class Program
    {
        static void Main(string[] args)
        {
            Vector3[] positions = new[]{
                new Vector3(0.5f,-0.5f,0.5f),
                new Vector3(-0.5f,-0.5f,0.5f),
                new Vector3(0.5f,0.5f,0.5f),
                new Vector3(-0.5f,0.5f,0.5f),
                new Vector3(0.5f,0.5f,-0.5f),
                new Vector3(-0.5f,0.5f,-0.5f),
                new Vector3(0.5f,-0.5f,-0.5f),
                new Vector3(-0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,0.5f,0.5f),
                new Vector3(-0.5f,0.5f,0.5f),
                new Vector3(0.5f,0.5f,-0.5f),
                new Vector3(-0.5f,0.5f,-0.5f),
                new Vector3(0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,-0.5f,0.5f),
                new Vector3(-0.5f,-0.5f,0.5f),
                new Vector3(-0.5f,-0.5f,-0.5f),
                new Vector3(-0.5f,-0.5f,0.5f),
                new Vector3(-0.5f,0.5f,0.5f),
                new Vector3(-0.5f,0.5f,-0.5f),
                new Vector3(-0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,0.5f,-0.5f),
                new Vector3(0.5f,0.5f,0.5f),
                new Vector3(0.5f,-0.5f,0.5f),
            };

            uint[] indices = { 0, 2, 3, 0, 3, 1, 8, 4, 5, 8, 5, 9, 10, 6, 7, 10, 7, 11, 12, 13, 14, 12, 14, 15, 16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23, };

            uint[] adj = MeshUtilities.GenerateAdjacencyAndPointReps(indices, positions);
            var result = UVAtlas.Create(positions, indices, adj);
        }
    }
}

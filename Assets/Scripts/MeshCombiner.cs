using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshCombiner
{
    public static Mesh CombineMeshesFromMeshFilters(MeshFilter[] meshFilters, bool combineSubMeshes = false, bool useMatrices = true)
    {
        List<Tuple<Mesh, Matrix4x4>> allMeshes = new List<Tuple<Mesh, Matrix4x4>>();
        foreach (var meshFilter in meshFilters)
        {
            if (meshFilter.mesh.subMeshCount > 1)
            {
                for (int i = 0; i < meshFilter.mesh.subMeshCount; i++)
                {
                    allMeshes.Add(new Tuple<Mesh, Matrix4x4>(SubmeshToMeshGreedy(meshFilter.mesh, i), meshFilter.transform.localToWorldMatrix));
                }
            }
            else
            {
                allMeshes.Add(new Tuple<Mesh, Matrix4x4>(meshFilter.mesh, meshFilter.transform.localToWorldMatrix));
            }
        }

        CombineInstance[] combine = new CombineInstance[allMeshes.Count];
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "Combined Mesh";
        combinedMesh.indexFormat = IndexFormat.UInt32;
        int idx = 0;
        foreach (var meshPair in allMeshes)
        {
            combine[idx].mesh = meshPair.Item1;
            combine[idx].transform = meshPair.Item2;
            idx++;
        }
        combinedMesh.CombineMeshes(combine, combineSubMeshes, useMatrices);
        combinedMesh.Optimize();
        return combinedMesh;
    }

    public static Mesh SubmeshToMeshGreedy(Mesh oldMesh, int index)
    {
        Mesh newMesh = new Mesh();
        newMesh.name = $"{oldMesh.name}_{index}";
        newMesh.vertices = oldMesh.vertices;
        newMesh.triangles = oldMesh.GetTriangles(index);
        newMesh.normals = oldMesh.normals;
        newMesh.uv = oldMesh.uv;
        newMesh.Optimize();
        return newMesh;
    }

    public static Mesh SubmeshToMesh(Mesh oldMesh, int index)
    {
        Mesh newMesh = new Mesh();

        newMesh.name = $"{oldMesh.name}_{index}";

        int[] triangles = oldMesh.GetTriangles(index);

        Vector3[] newVertices = new Vector3[triangles.Length];
        Vector2[] newUvs = new Vector2[triangles.Length];

        Dictionary<int, int> oldToNewIndices = new Dictionary<int, int>();
        int newIndex = 0;

        for (int i = 0; i < oldMesh.vertices.Length; i++)
        {
            if (triangles.Contains(i))
            {
                newVertices[newIndex] = oldMesh.vertices[i];
                newUvs[newIndex] = oldMesh.uv[i];
                oldToNewIndices.Add(i, newIndex);
                ++newIndex;
            }
        }

        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = oldToNewIndices[triangles[i]];
        }

        newMesh.vertices = newVertices.ToArray();
        newMesh.uv = newUvs.ToArray();
        newMesh.triangles = newTriangles;
        newMesh.Optimize();
        return newMesh;
    }
}
using UnityEngine;
using System;
public class Rocks {
    private Vector3[] vertices; 
    private Vector2[] uv; 
    private int[] triangles;

    // Make some rocks, which are really just a bunch of sphere for now.
    public void Generate(Vector3[] curVerts, int[] curTriangles, float scaler = .5F) { 

        float scale = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler / 3));
    
        vertices = curVerts;
        triangles = curTriangles;

        for (int i = 0; i <= vertices.Length - 1; i++) {
            vertices[i] *= scale;
        }
    }
    public Vector3[] GetVerts() {
        return vertices;
    }
    public int[] GetTris() {
        return triangles;
    }
    public Vector2[] GetUv() {
        return uv;
    }
}

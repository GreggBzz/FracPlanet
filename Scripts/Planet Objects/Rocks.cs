using UnityEngine;
using System;
public class Rocks {
    private Vector3[] vertices; 
    private Vector2[] uv; 
    private int[] triangles;

    // Make some rocks, which are really just a bunch of sphere for now.
    public void Generate(Vector3[] curVerts, int[] curTriangles, Vector2[] curUv, int[] dupeVerts, float scaler = .5F, int seed = 42) {
        
        float scaleX = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler * 10));
        float scaleY = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler * 10));
        float scaleZ = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler * 10));
        Vector3 scaleVector = new Vector3(scaleX, scaleY, scaleZ);

        float rotateX = UnityEngine.Random.Range(1, 360);
        float rotateY = UnityEngine.Random.Range(1, 360);
        float rotateZ = UnityEngine.Random.Range(1, 360);
        Quaternion rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
        

        vertices = curVerts;
        triangles = curTriangles;
        uv = curUv;

        float curDisplacement = UnityEngine.Random.Range(1.00F, 1.10F);
        for (int i = 0; i <= dupeVerts.Length - 1; i++) {
            if (dupeVerts[i] == -99) {
                curDisplacement = UnityEngine.Random.Range(1.00F, 1.10F);
                continue;
            }
            vertices[dupeVerts[i]] = Vector3.Scale(scaleVector, vertices[dupeVerts[i]]);
            vertices[dupeVerts[i]] *= curDisplacement;
            vertices[dupeVerts[i]] = rotation * vertices[dupeVerts[i]];
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

using UnityEngine;
using System;
public class Rocks {
    private Vector3[] vertices; 
    private Vector2[] uv; 
    private int[] triangles;

    // Make some rocks.
    public void Generate(Vector3[] curVerts, int[] curTriangles, Vector2[] curUv, int[] dupeVerts, float jagginess = 1.4F, float scaler = .5F, int seed = 42) {
        float scaleY, scaleX, scaleZ, rotateX, rotateY, rotateZ;
        if (jagginess < 1.2F) {
            scaleY = UnityEngine.Random.Range(scaler - (scaler / 10), scaler + (scaler * 3));
            rotateX = UnityEngine.Random.Range(1, 20);
            rotateZ = UnityEngine.Random.Range(1, 20);
            rotateY = UnityEngine.Random.Range(1, 360);
        }
        else {
            scaleY = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler * 10));
            rotateX = UnityEngine.Random.Range(1, 360);
            rotateZ = UnityEngine.Random.Range(1, 360);
            rotateY = UnityEngine.Random.Range(1, 360);
        }
      
        scaleX = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler * 10));
        scaleZ = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler * 10));
        Vector3 scaleVector = new Vector3(scaleX, scaleY, scaleZ);
        Quaternion rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
        vertices = curVerts;
        triangles = curTriangles;
        uv = curUv;

        float curDisplacement = UnityEngine.Random.Range(1.00F, jagginess);
        for (int i = 0; i <= dupeVerts.Length - 1; i++) {
            if (dupeVerts[i] == -99) {
                curDisplacement = UnityEngine.Random.Range(1.00F, jagginess);
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

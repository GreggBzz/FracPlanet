using UnityEngine;
using System;
public class Grass {
    private Vector3[] vertices = new Vector3[12];
    private Vector2[] uv = new Vector2[12];
    private Vector3[] normals = new Vector3[12];
    private int[] triangles = new int[18];

    // Make one bunch of billboard grass, three planes, offset by 60 degrees. 
    public void Generate(float scaler = .5F, float width = .8F, float height = 1.0F, int type = 0) {
        int numTextures = GrassManager.grassTextures;

        float scale = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler / 3));
        float angle;
        // 6 x 2 points around the circle, one high, one low, offset by a random angle;
        float angleOffset = UnityEngine.Random.Range(0f, (float)(2 * Math.PI));
        for (int i = 0; i <= 5; i++) {
            angle = (60 * i * Mathf.Deg2Rad) - angleOffset;
            vertices[i].z = vertices[11 - i].z = (float)(Math.Sin(angle)) * width;
            vertices[i].x = vertices[11 - i].x = (float)(Math.Cos(angle)) * width;
            vertices[i].y = 0; vertices[11 - i].y = height;
            // set normals to up for better lighting.
            normals[i] = normals[11 - i] = Vector3.up;
        }

        // assemble the triangles along each plane. Culling is off in the shader so we don't 
        // have to worry about assembling a backside.
        triangles[0] = 0; triangles[1] = 11; triangles[2] = 3;
        triangles[3] = 11; triangles[4] = 8; triangles[5] = 3;
        triangles[6] = 1; triangles[7] = 10; triangles[8] = 4;

        triangles[9] = 10; triangles[10] = 7; triangles[11] = 4;
        triangles[12] = 5; triangles[13] = 6; triangles[14] = 2;
        triangles[15] = 6; triangles[16] = 9; triangles[17] = 2;


        // texture and scale, we use a texture strip to enhance batching.
        float xSegment = 1.0F / (float)numTextures;
        float xMin = type * xSegment;
        float xMax = (type + 1) * xSegment; 


        uv[0] = uv[1] = uv[5] = new Vector2(xMin, 0F);
        uv[6] = uv[10] = uv[11] = new Vector2(xMin, 1F);
        uv[2] = uv[4] = uv[3] = new Vector2(xMax, 0F);
        uv[9] = uv[7] = uv[8] = new Vector2(xMax, 1F);

        for (int i = 0; i <= 11; i++) {
            vertices[i] *= scale;
        }
    }


    public Vector3[] GetVerts () {
        return vertices;
    }
    public int[] GetTris() {
        return triangles;
    }
    public Vector2[] GetUv() {
        return uv;
    }
    // get and set the normals so unity calculates them runtime. https://docs.unity3d.com/ScriptReference/Mesh-normals.html
    public Vector3[] GetNormals() {
        return normals;
    }
    public void SetNormals(Vector3[] mesh_normals) {
        normals = mesh_normals;
    }
}

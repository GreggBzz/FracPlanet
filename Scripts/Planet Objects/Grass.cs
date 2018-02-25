using UnityEngine;
using System;
public class Grass {
    private Vector3[] vertices = new Vector3[7];
    private Vector2[] uv = new Vector2[7];
    private Vector3[] normals = new Vector3[7];
    private int[] triangles = new int[9];

    // Make one bnunch of billboard grass, three planes, each a triangle, offset by 60 degrees. 
    public void Generate(float scaler = 1.2F, float width = 1.2F, float height = 1.2F, float variety = 3F) {

        float scale = UnityEngine.Random.Range(scaler - (scaler / 3), scaler + (scaler / 3));
        float angle;
        // 6 points around the circle. 
        for (int i = 1; i <= 6; i++) {
            angle = 60 * i * Mathf.Deg2Rad;
            vertices[i - 1].z = (float)(Math.Sin(angle)) * width;
            vertices[i - 1].x = (float)(Math.Cos(angle)) * width;
            vertices[i - 1].y = 0;
            // set normals to "mostly" up, but add a little wiggle for variety.
            normals[i - 1] = new Vector3(UnityEngine.Random.Range(0F, .2F), 1.0F, UnityEngine.Random.Range(0F, .2F));
        }
        // on point in the middle.
        vertices[6] = new Vector3(0F, height, 0F);

        // assemble the triangles along each plane. Culling is off in the shader so we don't 
        // have to worry about assembling a backside.
        triangles[0] = 0; triangles[1] = 6; triangles[2] = 3;
        triangles[3] = 1; triangles[4] = 6; triangles[5] = 4;
        triangles[6] = 2; triangles[7] = 6; triangles[8] = 5;

        // texture and scale.
        for (int i = 0; i <= 2; i++) {
            vertices[i] *= scale;
            uv[i].x = 0F; uv[i].y = 0F;
            vertices[i + 3] *= scale;
            uv[i + 3].x = 1F; uv[i].y = 0F;
        }
        uv[6].x = .5F; uv[6].y = 1F;
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

using UnityEngine;
//using System;
public class Grass {
    private Vector3[] vertices = new Vector3[5];
    private Vector2[] uv = new Vector2[5];
    private int[] triangles = new int[12];

    // Make one bnunch of billboard grass, two, two faced triangles, offset by 90 degrees. 
    public void Generate(float scaler=1.5F, float width=1F, float height=1F, float skew=.4F) {
        // Setup random angle of skew, with 2 x skew being the highest potential coordinate diff.
        float scale = Random.Range(scaler - (scaler / 3), scaler + (scaler / 3));
        float lSkew = Random.Range(-skew, skew);
        float rSkew = (lSkew * -1F);
        // verts.

        vertices[0].x = -width/2F; vertices[0].y = 0F; vertices[0].z = lSkew; vertices[0] *= scale; // the bottom left
        uv[0].x = 0F; uv[0].y =  0F;
        vertices[2].x = width/2F; vertices[2].y = 0F; vertices[2].z = rSkew; vertices[2] *= scale; // the bottom right
        uv[2].x = 1F; uv[2].y = 0F;

        vertices[1].x = 0F; vertices[1].y = height; vertices[1].z = Random.Range(-skew / 8F, +skew / 8F); vertices[1] *= scale;  // the top point
        uv[1].x = .5F; uv[1].y = 1F;

        vertices[3].x = rSkew; vertices[3].y = 0F; vertices[3].z = -width / 2F; vertices[3] *= scale; // the close bottom center
        uv[3].x = 1F; uv[3].y = 0F;
        vertices[4].x = lSkew; vertices[4].y = 0F; vertices[4].z = width / 2F; vertices[4] *= scale; // the far bottom center
        uv[4].x = 0F; uv[4].y = 0F;

        // tris
        triangles[0] = 0; triangles[1] = 1; triangles[2] = 2;
        triangles[3] = 2; triangles[4] = 1; triangles[5] = 0;

        triangles[6] = 3; triangles[7] = 1; triangles[8] = 4;
        triangles[9] = 4; triangles[10] = 1; triangles[11] = 3;
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
}

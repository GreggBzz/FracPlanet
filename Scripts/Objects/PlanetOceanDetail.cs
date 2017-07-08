using System;
using System.Linq;
using UnityEngine;

public class PlanetOceanDetail : MonoBehaviour {
    // keep track of how many new vertices we've added during tesselate.
    // and the parent vertices, the center of each hexagon.
    private int vertCount;

    private int debugCount;

    private int[] triangles;
    private int[] tempTriangles;

    // scale variables.
    private double radius;
    private float pScale;

    // mesh tesselation setup is done in the geometry class.
    private PlanetGeometry meshGeometry;
    
    private Vector3[] vertices = new Vector3[40962];
    private Vector3[] tmpVerticies;

    public void Generate(int[] curTriangles, Vector3[] curVerts, float curDiameter, bool bottom = false) {

        meshGeometry = gameObject.AddComponent<PlanetGeometry>();

        int triCount = 0;
        int[] vertexRef = new int[40962];
        Vector3[] tmpVerts = new Vector3[40962];
        int[] tmpTris = new int[245000];

        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            // mark the top 1/100th of the ocean to keep.
            if ((curVerts[curTriangles[i]].y) > (curDiameter / 2F - curDiameter / 100F) && (!bottom)) {
                // if the vertext hasn't been copied, mark it in the refrence array (vertxRef[oldVerti] = NewVerti)
                // and copy it. 
                if (vertexRef[curTriangles[i]] == 0) {
                    vertexRef[curTriangles[i]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i]];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 1]] == 0) {
                    vertexRef[curTriangles[i + 1]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 1]];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 2]] == 0) {
                    vertexRef[curTriangles[i + 2]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 2]];
                    vertCount += 1;
                }
                tmpTris[triCount] = vertexRef[curTriangles[i]];
                tmpTris[triCount + 1] = vertexRef[curTriangles[i + 1]];
                tmpTris[triCount + 2] = vertexRef[curTriangles[i + 2]];
                triCount += 3;
            }
            if ((curVerts[curTriangles[i]].y) < (curDiameter / 2F - curDiameter / 110F) && (bottom)) {
                // if the vertext hasn't been copied, mark it in the refrence array (vertxRef[oldVerti] = NewVerti)
                // and copy it. 
                if (vertexRef[curTriangles[i]] == 0) {
                    vertexRef[curTriangles[i]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i]];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 1]] == 0) {
                    vertexRef[curTriangles[i + 1]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 1]];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 2]] == 0) {
                    vertexRef[curTriangles[i + 2]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 2]];
                    vertCount += 1;
                }
                tmpTris[triCount] = vertexRef[curTriangles[i]];
                tmpTris[triCount + 1] = vertexRef[curTriangles[i + 1]];
                tmpTris[triCount + 2] = vertexRef[curTriangles[i + 2]];
                triCount += 3;
            }
        }
        triangles = new int[triCount];

        for (int i = 0; i <= triCount - 1; i++) {
            triangles[i] = tmpTris[i];
        }
        for (int i = 0; i <= vertCount - 1; i++) {
            vertices[i] = tmpVerts[i];
        }

        // clean up
        tmpVerts = null; tmpTris = null; vertexRef = null;

        // if we're just truncating the bottom, return. Otherwise, tesselate the top
        if (bottom) {
            return;
        }

        meshGeometry.newVertIndex = vertCount;
        meshGeometry.vertices = vertices;
        meshGeometry.triangles = triangles;
        meshGeometry.tessRounds = 1;
        meshGeometry.Generate("ocean", curDiameter, 100, false);

        triangles = meshGeometry.GetTriangles();
        vertices = meshGeometry.GetVerts();
        vertCount = meshGeometry.GetVertIndex();
    }

    public int[] GetTris(bool reverse = false) {
        if (reverse) {
            InvertTriangles();
         }
        return triangles;
    }

    public Vector2[] GetUV() {
        Vector2[] uv = new Vector2[vertCount];
        System.Random rnd = new System.Random();
        for (int i = 0; i <= vertCount - 1; i++) {
            uv[i].x = (float)rnd.NextDouble();
            uv[i].y = (float)rnd.NextDouble();
        }
        return uv;
    }

    private void InvertTriangles() {
        triangles = triangles.Reverse().ToArray();
    }

    public Vector3[] GetVerts() {
        tmpVerticies = new Vector3[vertCount];
        // assign the verts to a properly sized array.
        for (int i = 0; i <= vertCount - 1; i++) {
            tmpVerticies[i] = vertices[i];
        }
        vertices = null;
        return tmpVerticies;
    }
}

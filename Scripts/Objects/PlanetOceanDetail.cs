using System;
using System.IO;
using UnityEngine;

public class PlanetOceanDetail : MonoBehaviour {
    // keep track of how many new vertices we've added during tesselate.
    // and the parent vertices, the center of each hexagon.
    private int newVertIndex;
    private int vertCount;

    private int debugCount;

    private int[] triangles;
    private int[] tempTriangles;

    // scale variables.
    private double radius;
    private float pScale;

    private struct doneMidpoint {
        public int adjVert;
        public int midPoint;
    }

    // mesh tesselation setup is done in the geometry class.
    private PlanetGeometry meshGeometry;

    private doneMidpoint[,] doneMidpoints = new doneMidpoint[40962, 6];
    private Vector3[] vertices = new Vector3[40962];

    public void Generate(int[] curTriangles, Vector3[] curVerts, float curDiameter) {

        meshGeometry = gameObject.AddComponent<PlanetGeometry>();

        int triCount = 0;
        int[] vertexRef = new int[65000];
        Vector3[] tmpVerts = new Vector3[2000];
        int[] tmpTris = new int[10000];

        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            // mark the top 1/100th of the ocean to keep.
            if ((curVerts[curTriangles[i]].y) > (curDiameter / 2F - curDiameter / 100F)) {
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
            newVertIndex = (ushort)vertCount;
        }

        tmpVerts = null; tmpTris = null; vertexRef = null;

        meshGeometry.newVertIndex = newVertIndex;
        meshGeometry.vertices = vertices;
        meshGeometry.triangles = triangles;
        meshGeometry.tessRounds = 1;
        meshGeometry.Generate("ocean", curDiameter, 100, false);

        triangles = meshGeometry.GetTriangles();
        vertices = meshGeometry.GetVerts();
        newVertIndex = meshGeometry.GetVertIndex();
    }

    public int[] GetTris() {
        return triangles;
    }

    public Vector3[] GetVerts() {
        Vector3[] tmpVerticies = new Vector3[newVertIndex];
        float maxY = 0F;
        // assign the verts to a properly sized array.
        // flatten the partial ocean.
        for (int i = 0; i <= newVertIndex - 1; i++) {
            if (vertices[i].y > maxY) { maxY = vertices[i].y; }
        }
        for (int i = 0; i <= newVertIndex - 1; i++) {
            tmpVerticies[i] = vertices[i];
           // tmpVerticies[i].y = maxY;
        }
        vertices = null;
        return tmpVerticies;
    }
}

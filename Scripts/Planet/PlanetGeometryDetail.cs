using System;
using UnityEngine;

public class PlanetGeometryDetail : MonoBehaviour {

    // vert adjacentcies, used when making rivers.
    private struct adjacent {
        public int vert;
        public short count;
    }

    private adjacent[,] adjacents;
    private Vector3[] newVertices;
    private int[] newTriangles;

    private System.Random rnd;

    private float maxHeight;
    private float minHeight;
    private float averageHeight;
    private float waterLine;

    // Use this for initialization
    void Start() {

    }
    
    public void Initialize(int[] triangles, Vector3[] vertices, int vertCount, float radius, int seed) {
        rnd = new System.Random(seed);
        //
        adjacents = new adjacent[vertCount, 6];
        FindAdjacents(triangles, vertCount);
        newVertices = vertices;
        waterLine = radius;
        SetHeights();

        // Make a few platues. 
        //for (int i = 0; i <= 5; i++) {
        //    int curVertIndex = adjacents[100, i].vert;
        //    if (curVertIndex == -99) { break; }
        //    Vector3 curVert = vertices[curVertIndex];
        //    float currentLength = (float)Math.Sqrt((curVert.x * curVert.x) + (curVert.y * curVert.y) + (curVert.z * curVert.z));
        //    float lengthMult = ((float)radius / currentLength);
        //    vertices[curVertIndex] = vertices[curVertIndex] * (lengthMult + .02F);
        //}
    }

    private void SetHeights() {
        float heightSum = 0F;
        float curHeight = 0F;
        maxHeight = (float)Math.Sqrt((newVertices[0].x * newVertices[0].x) + (newVertices[0].y * newVertices[0].y) + (newVertices[0].z * newVertices[0].z));
        minHeight = (float)Math.Sqrt((newVertices[0].x * newVertices[0].x) + (newVertices[0].y * newVertices[0].y) + (newVertices[0].z * newVertices[0].z));
        for (int i = 0; i <= newVertices.Length - 1; i++) {
            curHeight = (float)Math.Sqrt((newVertices[i].x * newVertices[i].x) + (newVertices[i].y * newVertices[i].y) + (newVertices[i].z * newVertices[i].z));
            heightSum += curHeight;
            if (curHeight < minHeight) { minHeight = curHeight; }
            if (curHeight > maxHeight) { maxHeight = curHeight; }
        }
        averageHeight = heightSum / newVertices.Length;

        Debug.Log("Max Height: " + maxHeight);
        Debug.Log("Min Height: " + minHeight);
        Debug.Log("Average Height: " + averageHeight);
        Debug.Log("Water Line: " + waterLine);
    }

    private void FindAdjacents(int[] tris, int vertCount) {
        // find a verts six or sometimes five neighbors.
        // stuff those into our 2d refrence array.
        for (int i = 0; i <= vertCount - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                adjacents[i, i2].vert = -99;
                adjacents[i, i2].count = 0;
            }
        }
        for (int i = 0; i <= tris.Length - 1; i += 3) {
            MarkAdjacent(tris[i], tris[i + 1], tris[i + 2]);
            MarkAdjacent(tris[i + 1], tris[i], tris[i + 2]);
            MarkAdjacent(tris[i + 2], tris[i], tris[i + 1]);
        }
    }

    private void MarkAdjacent(int vert1, int vert2, int vert3) {
        bool skip2 = false; bool skip3 = false;
        short count = adjacents[vert1, 0].count;
        for (int i = 0; i <= 5; i++) {
            if (adjacents[vert1, i].vert == vert2) { skip2 = true; }
            if (adjacents[vert1, i].vert == vert3) { skip3 = true; }
        }
        if (!skip2) {
            adjacents[vert1, count].vert = vert2;
            count += 1;
            if (count == 6) { return; }
        }
        if (!skip3) {
            adjacents[vert1, count].vert = vert3;
            count += 1;
            if (count == 6) { return; }
        }
        adjacents[vert1, 0].count = count;
    }

    private void MakeRiver() {
        float curHeight = 0F;
        int curVert = 0;
        int lastVert = 0;

        for (int i = 0; i <= newVertices.Length - 1; i++) {
            curHeight = (float)Math.Sqrt((newVertices[i].x * newVertices[i].x) + (newVertices[i].y * newVertices[i].y) + (newVertices[i].z * newVertices[i].z));
            // pick a random starting vert in the top 10% of height.
            if (curHeight > (minHeight + (maxHeight - minHeight) * .9F)) {
                if (rnd.NextDouble() > .95F) {
                    curVert = i;
                    lastVert = i;
                    break;
                }
                else {
                    curVert = i;
                    lastVert = i;
                }
            }
            // until we're in the ocean.
            // pick the lowest neighboring vert and drop it down, move on.
        }
    }

    private void MakePlain() {
        // Make a large flat area.
    }

    private void MakeMountain() {
        // Make a very high collection of neighboring verts.
    }
}

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
    private bool[] riverVerts;
    private bool[] mountainVerts;
    private bool[] plainVerts;

    private System.Random rnd;

    public float maxHeight;
    public float minHeight;
    public float averageHeight;
    public float waterLine;

    // Use this for initialization
    void Start() {

    }
    
    public void Initialize(int[] triangles, Vector3[] vertices, int vertCount, float radius, int seed) {
        rnd = new System.Random(seed);
        adjacents = new adjacent[vertCount, 6];
        FindAdjacents(triangles, vertCount);
        newVertices = vertices;
        waterLine = radius;
        ResetHeights();
        // Mountains
        int numMountains = rnd.Next(20, 50);
        mountainVerts = new bool[newVertices.Length];
        for (int i = 0; i <= numMountains; i++) {
            MakeMountain();
        }
        // Plains
        int numPlains = rnd.Next(20, 70);
        plainVerts = new bool[newVertices.Length];
        for (int i = 0; i <= numPlains; i++) {
            MakePlain();
        }
        // Rivers
        int numRivers = rnd.Next(20, 70);
        riverVerts = new bool[newVertices.Length];
        for (int i = 0; i <= numRivers; i++) {
            MakeRiver();
        }
        ResetHeights();
    }

    private void ResetHeights() {
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
        float nextHeight = 0F;

        float maxDisplacement = .9995F;
        float minDisplacement = .9990F;

        int nextVert = 0;
        int candidateVert = 0;
        int curVert = 0;

        int maxCount = 0;

        do {
            candidateVert = rnd.Next(newVertices.Length - 1);
            if (riverVerts[candidateVert]) { maxCount += 1; continue; }
            curHeight = (float)Math.Sqrt((newVertices[candidateVert].x * newVertices[candidateVert].x) +
                                         (newVertices[candidateVert].y * newVertices[candidateVert].y) +
                                         (newVertices[candidateVert].z * newVertices[candidateVert].z));
            if (curHeight > (minHeight + (maxHeight - minHeight) * .75F)) {
                curVert = candidateVert;
                break;
            }
            maxCount += 1;
        } while (maxCount < 100);
        
        if (candidateVert == 0) { return; }

        riverVerts[curVert] = true;
        // check for the lowest neighboring vert, move to that vert, check again, until we get below the waterline.
        // keep track of which verts we've checked in the large boolean array, so we don't circle back.
        newVertices[curVert] *= (float)(rnd.NextDouble() * (maxDisplacement - minDisplacement) + minDisplacement);
        do {
            // look at the neighbors, pick the lowest, skipping already checked verts.
            curHeight = maxHeight + 1000F;
            for (int i = 0; i <= 5; i++) {
                if ((adjacents[curVert, i].vert == -99) || (riverVerts[(adjacents[curVert, i].vert)])) { continue; }
                nextVert = adjacents[curVert, i].vert;
                nextHeight = (float)Math.Sqrt((newVertices[nextVert].x * newVertices[nextVert].x) +
                                         (newVertices[nextVert].y * newVertices[nextVert].y) +
                                         (newVertices[nextVert].z * newVertices[nextVert].z));
                // nominate a vert for lowest neighbor.
                if (nextHeight < curHeight) { candidateVert = nextVert; curHeight = nextHeight; }
                riverVerts[nextVert] = true;
            }
            // candidateVert wins!
            curVert = candidateVert;
            curHeight = (float)Math.Sqrt((newVertices[curVert].x * newVertices[curVert].x) +
                                         (newVertices[curVert].y * newVertices[curVert].y) +
                                         (newVertices[curVert].z * newVertices[curVert].z));
            riverVerts[curVert] = true;

            newVertices[curVert] *= (float)(rnd.NextDouble() * (maxDisplacement - minDisplacement) + minDisplacement);
        } while (curHeight > waterLine);
    }

    private void MakeMountain() {
        // Make a very high collection of neighboring verts.
        float curHeight = 0F;
        // mountain parameters. 
        float maxDisplacement = 1.001F;
        float minDisplacement = 1.0005F;
        // mountain size in number of verts displaced.
        int mountainSize = 0;
        int candidateVert = 0;
        int curVert = 0;
        // attempts for picking random stuff.
        int maxCount = 0;

        do {
            candidateVert = rnd.Next(newVertices.Length - 1);
            if (mountainVerts[candidateVert]) { maxCount += 1; continue; }
            curHeight = (float)Math.Sqrt((newVertices[candidateVert].x * newVertices[candidateVert].x) +
                                         (newVertices[candidateVert].y * newVertices[candidateVert].y) +
                                         (newVertices[candidateVert].z * newVertices[candidateVert].z));
            // 25% chance we make an underwater moutain.
            if ((curHeight < waterLine) && (rnd.NextDouble() < .25f)) {
                curVert = candidateVert;
                break;
            }
            // 75% chance we make an above water mountain.
            if ((curHeight > waterLine) && (rnd.NextDouble() < .75f)) {
                curVert = candidateVert;
                break;
            }
            maxCount += 1;
        } while (maxCount < 100);

        maxCount = 0;
        mountainSize = rnd.Next(20, 200);

        mountainVerts[curVert] = true;
        newVertices[curVert] *= (float)(rnd.NextDouble() * (maxDisplacement - minDisplacement) + minDisplacement);

        do {
            for (int i = 0; i <= 5; i++) {
                if ((adjacents[curVert, i].vert == -99) || (mountainVerts[(adjacents[curVert, i].vert)])) { continue; }
                newVertices[adjacents[curVert, i].vert] *= (float)(rnd.NextDouble() * (maxDisplacement - minDisplacement) + minDisplacement);
                mountainVerts[adjacents[curVert, i].vert] = true;
                maxCount += 1;
                if (maxCount > mountainSize) { break; }
            }
            curVert = adjacents[curVert, rnd.Next(0, 4)].vert;
            maxDisplacement -= .00001f;
        } while (maxCount < mountainSize);
    }

    private void MakePlain() {
        // make a flat collection of neighboring verts.
        float curHeight = 0F;
        // plain parameters. 
        int plainSize = 0;
        float plainHeight = 0F;
        int candidateVert = 0;
        int curVert = 0;
        // attempts for picking random stuff.
        int maxCount = 0;

        do {
            candidateVert = rnd.Next(newVertices.Length - 1);
            if ((plainVerts[candidateVert]) || (mountainVerts[candidateVert])) { maxCount += 1; continue; }
            // 10% chance we make an underwater plain.
            if ((curHeight < waterLine) && (rnd.NextDouble() < .10f)) {
                curVert = candidateVert;
                plainHeight = (float)Math.Sqrt((newVertices[curVert].x * newVertices[curVert].x) +
                                              (newVertices[curVert].y * newVertices[curVert].y) +
                                              (newVertices[curVert].z * newVertices[curVert].z));
                break;
            }
            // 90% chance we make an above water plain.
            if ((curHeight > waterLine) && (rnd.NextDouble() < .90f)) {
                curVert = candidateVert;
                plainHeight = (float)Math.Sqrt((newVertices[curVert].x * newVertices[curVert].x) +
                                              (newVertices[curVert].y * newVertices[curVert].y) +
                                              (newVertices[curVert].z * newVertices[curVert].z));
                break;
            }
            maxCount += 1;
        } while (maxCount < 100);

        maxCount = 0;
        plainSize = rnd.Next(20, 150);
        plainVerts[curVert] = true;

        do {
            for (int i = 0; i <= 5; i++) {
                if ((adjacents[curVert, i].vert == -99) || (plainVerts[(adjacents[curVert, i].vert)])) { continue; }
                curHeight = (float)Math.Sqrt((newVertices[adjacents[curVert, i].vert].x * newVertices[adjacents[curVert, i].vert].x) + 
                                             (newVertices[adjacents[curVert, i].vert].y * newVertices[adjacents[curVert, i].vert].y) + 
                                             (newVertices[adjacents[curVert, i].vert].z * newVertices[adjacents[curVert, i].vert].z));
                float lengthMult = (plainHeight / curHeight);
                newVertices[adjacents[curVert, i].vert] *= lengthMult;
                plainVerts[adjacents[curVert, i].vert] = true;
                maxCount += 1;
                if (maxCount > plainSize) { break; }
            }
            if (adjacents[curVert, 5].vert == -99) {
                curVert = adjacents[curVert, rnd.Next(0, 4)].vert;
            }
            else { 
                curVert = adjacents[curVert, rnd.Next(0, 5)].vert;
            }

        } while (maxCount < plainSize);
    }

    public Vector3[] GetVerts() {
        return newVertices;
    }
}

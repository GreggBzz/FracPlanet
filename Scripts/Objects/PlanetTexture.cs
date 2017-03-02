using System.Collections;
using UnityEngine;
using System;

public class PlanetTexture : MonoBehaviour {

    // fake waves.    
    private bool[] waveDirection;
    private float[] waveSize;
    public bool skipframe = false;
    public float tideStrength = 0.0F;

    private struct adjacent {
        public int vert;
        public short count;
    }

    private adjacent[,] adjacents;
    private Vector2[] uv;

    // A simple, per verticie method.
    public Vector2[] TextureTerra(int vertCount, int parentVertCount, Vector3[] vertices, float radius, int[] triangles) {
        adjacents = new adjacent[vertCount, 6];
        uv = new Vector2[vertCount];
        float[] vertLength = new float[vertCount];
        float maxElev = 0F;
        float minElev = radius * 10;

        for (int i = 0; i <= vertCount - 1; i++) {
            uv[i].x = -10F;
            uv[i].y = -10F;
        }

        // find the maximum and minimum elevations.
        for (int i = 0; i <= vertices.Length - 1; i++) {
            vertLength[i] = (float)Math.Sqrt((vertices[i].x * vertices[i].x) +
                                             (vertices[i].y * vertices[i].y) +
                                             (vertices[i].z * vertices[i].z));
            if (vertLength[i] <= minElev) { minElev = vertLength[i]; }
            if (vertLength[i] >= maxElev) { maxElev = vertLength[i]; }
        }

        // assign all centers to the far right of the texture map.
        // for more horizontal texture resolution.
        for (int i = 0; i <= parentVertCount - 1; i++) {
            float uvyCenter = (vertLength[i] - minElev) / (maxElev - minElev);
            uv[i].x = 1F; uv[i].y = uvyCenter;
        }
        // assign all adjacent verts to the far left of the texture map.
        for (int i = parentVertCount; i <= vertCount - 1; i++) {
            float uvyCenter = (vertLength[i] - minElev) / (maxElev - minElev);
            uv[i].x = 0F; uv[i].y = uvyCenter;
        }
        return uv;
    }

    private void findAdjacents(int[] tris, int vertCount) {
        for (int i = 0; i <= vertCount - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                adjacents[i, i2].vert = -99;
                adjacents[i, i2].count = 0;
            }
        }
        for (int i = 0; i <= tris.Length - 1; i += 3) {
            markAdjacent(tris[i], tris[i + 1], tris[i + 2]);
            markAdjacent(tris[i + 1], tris[i], tris[i + 2]);
            markAdjacent(tris[i + 2], tris[i], tris[i + 1]);
        }
    }

    private void markAdjacent(int vert1, int vert2, int vert3) {
        bool skip2 = false; bool skip3 = false;
        short count = adjacents[vert1, 0].count;
        for (int i = 0; i <= 5; i++) {
            if (adjacents[vert1, i].vert == vert2) { skip2 = true; }
            if (adjacents[vert1, i].vert == vert3) { skip3 = true; }
        }
        if (!skip2) {
            adjacents[vert1, count].vert = vert2;
            count += 1;
        }
        if (!skip3) {
            adjacents[vert1, count].vert = vert3;
            count += 1;
        }
        adjacents[vert1, 0].count = count;
    }

    private int[] markVertsToSkip(adjacent adjacnest) {
        int[] skipme = new int[60];
        for (int i = 0; i <= 59; i++) {

        }
        return skipme;
    }


    public Vector2[] TextureOcean(int vertCount, int parentVertCount, Vector3[] vertices, int[] triangles) {
        // A texturing method that does it's best to seamlessly texture tile our irregular polyhedra without
        // using square tiles, not pentagon/hexagon tiles. This was hard but I get about 98% correct with 4 UV points.
        adjacents = new adjacent[vertCount, 6];
        uv = new Vector2[vertCount];
        // 
        for (int i = 0; i <= vertCount - 1; i++) {
            uv[i].x = -10F;
            uv[i].y = -10F;
        }
        // find each vertices' 6, or sometimes 5 neighbors.
        findAdjacents(triangles, vertCount);
        // Not texturing. Setup random sine-ish waves for the ocean affects.
        InitializeWaves(vertCount);

        bool[] checkedMatches = new bool[3];
        int tmpVert;
        int adjVert;
        int centVert;
        int doneVerts = 0;

        centVert = 0;
        for (int i = 0; i <= parentVertCount - 1; i++) {
            uv[i].x = 0F; uv[i].y = 0F;
            doneVerts += 1;
        }
        do {
            for (int i2 = 0; i2 <= 5; i2++) {
                // break if adjvert was a child of the pentagons club.
                adjVert = adjacents[centVert, i2].vert;
                if (adjVert == -99) continue;
                // skip if we've already done this vert.
                if (uv[adjVert].x != -10) continue;
                checkedMatches = checkAdjacents(adjVert);
                uv[adjVert] = assignAdjacent(checkedMatches);
                doneVerts += 1;
            }
            // pick a new center.
            do {
                tmpVert = adjacents[centVert, UnityEngine.Random.Range(0, 6)].vert;
            } while (tmpVert == -99);
            centVert = tmpVert;
         } while (doneVerts <= 40960);
        // attempt one more time to reassign any failed verts.
        for (int i = 0; i <= vertCount - 1; i++) { 
            if (uv[i] == new Vector2(.5F, .5F)) {
                uv[i] = reassignFailedVert(i, adjacents);
            }
        }
        return uv;
    }

    private Vector2 reassignFailedVert(int vert, adjacent[,] adjacents) {
        // Try assigning adjacent vertices again.
        int adjVert;
        bool[] checkedMatches = new bool[3];
        // 'reverse' our adjacents.
        for (int i = 0; i <= 5; i++) {
            adjVert = adjacents[vert, i].vert;
            if (adjVert == -99) { break; }
            checkedMatches = checkAdjacents(adjVert);
            uv[adjVert] = assignAdjacent(checkedMatches, true);
        }
        // try the failed vert again.
        checkedMatches = checkAdjacents(vert);
        return assignAdjacent(checkedMatches);
    }

    private Vector2 assignAdjacent(bool[] checkedMatches, bool reverse = false) {
        // Assign adjacent vertices to 1 of 4 UV texture points.
        if (reverse) {
            if (!checkedMatches[3]) { return new Vector2(1F, 1F); }
            if (!checkedMatches[2]) { return new Vector2(0F, 1F); }
            if (!checkedMatches[1]) { return new Vector2(1F, 0F); }
            if (!checkedMatches[0]) { return new Vector2(0F, 0F); }
            return new Vector2(UnityEngine.Random.Range(0F, 1F), UnityEngine.Random.Range(0F, 1F));
        }
        if (!checkedMatches[0]) { return new Vector2(0F,0F); }
        if (!checkedMatches[1]) { return new Vector2(1F,0F); }
        if (!checkedMatches[2]) { return new Vector2(0F,1F); }
        if (!checkedMatches[3]) { return new Vector2(1F,1F); }
        return new Vector2(UnityEngine.Random.Range(0F, 1F), UnityEngine.Random.Range(0F, 1F));
   } 

    private bool[] checkAdjacents(int vert) {
        bool[] matches = new bool[4];
        for (int i = 0; i <= 5; i++) {
            if (adjacents[vert, i].vert == -99) {
                return matches;
            }
            if ((uv[adjacents[vert, i].vert].x + uv[adjacents[vert, i].vert].y) == 0F) {  
                matches[0] = true; continue;
            }
            if ((uv[adjacents[vert, i].vert].x == 1F) && (uv[adjacents[vert, i].vert].y == 0F)) {
                matches[1] = true; continue;
            }
            if ((uv[adjacents[vert, i].vert].x == 0F) && (uv[adjacents[vert, i].vert].y == 1F)) {
                matches[2] = true; continue;
            }
            if ((uv[adjacents[vert, i].vert].x + uv[adjacents[vert, i].vert].y) == 2F) {
                matches[3] = true; continue;
            }
        }
        return matches;
    }

    public Vector3[] MakeWaves(Vector3[] vertices, Vector3 center) {
        // a simple wave function. Let's move this to a shader, later.
        for (int i = 0; i <= vertices.Length - 1; i += 1) {
            if (waveDirection[i]) {
                vertices[i] = vertices[i] * (1.0F + (.00002F / (float)Math.Sqrt(Math.Abs(waveSize[i]))));
                waveSize[i] += .4F;
            }
            else 
            {
                vertices[i] = vertices[i] / (1.0F + (.00002F / (float)Math.Sqrt(Math.Abs(waveSize[i]))));
                waveSize[i] += -.4F;
            }
            if (Math.Abs(waveSize[i]) > 50F) waveDirection[i] = !waveDirection[i];
        }
        tideStrength += Time.deltaTime;
        if (tideStrength > 7F) {
            SetTide();
            tideStrength = 0F;
        }
        return vertices;
    }

    private void SetTide() {
        for (int i = 0; i <= waveDirection.Length - 1; i += 1) {
            // invert one of ten vertices.
            if (UnityEngine.Random.value > 0.2f) waveDirection[i] = !waveDirection[i];
        }
    }

    private void InitializeWaves(int vertCount) {
        waveDirection = new bool[vertCount];
        waveSize = new float[vertCount];
        for (int i = 0; i <= vertCount - 1; i += 1) {
            waveDirection[i] = (UnityEngine.Random.value > 0.5f);
            if (waveDirection[i]) {
                waveSize[i] = (UnityEngine.Random.value * 30F) + .1F;
            }
            else {
                waveSize[i] = -1F * ((UnityEngine.Random.value * 30F) + .1F);
            }
        }
    }

}

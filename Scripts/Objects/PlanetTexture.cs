using UnityEngine;
using System;

public class PlanetTexture : MonoBehaviour {
    // texture manager class. Methods to texture terrain, ocean, atmosphere (and clouds with a helper class).
    private PlanetCloud cloudTextureManager;

    private struct adjacent {
        public int vert;
        public short count;
    }

    private adjacent[,] adjacents;
    private Vector2[] uv;
    public float maxElev = 0F;

    private bool[] checkAdjacents(int vert) {
        // checks neighboring verts, returns an array of booleans to let us know which are nearby.
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

    private void findAdjacents(int[] tris, int vertCount) {
        // find a verts six or sometimes five neighbors.
        // stuff those into our 2d refrence array.
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

    private Vector2 assignAdjacent(bool[] checkedMatches, int permutation) {
        // Assign a vert that doesn't match our neighbors. 
        // Options for the permutation and order of assignment, when there's more than once choice.
        Vector2[] returnVectors = new Vector2[4];
        returnVectors[0] = new Vector2(0F, 0F);
        returnVectors[1] = new Vector2(1F, 0F);
        returnVectors[2] = new Vector2(0F, 1F);
        returnVectors[3] = new Vector2(1F, 1F);
        int curPermutation = 0;
        for (int i = 0; i <= 3; i++) {
            for (int j = 0; j <= 3; j++) {
                if (j == i) continue;
                for (int k = 0; k <= 3; k++) {
                    if ((k == j) || (k == i)) continue;
                    for (int l = 0; l <= 3; l++) {
                        if ((l == i) || (l == j) || (l == k)) continue;
                        if (curPermutation == permutation) {
                            if (!checkedMatches[i]) { return returnVectors[i]; }
                            if (!checkedMatches[j]) { return returnVectors[j]; }
                            if (!checkedMatches[k]) { return returnVectors[k]; }
                            if (!checkedMatches[l]) { return returnVectors[l]; }
                        }
                        curPermutation += 1;
                    }
                }
            }
        }
        return new Vector2(.5F, .5F);
    }

    public Vector2[] Texture(int vertCount, int parentVertCount, Vector3[] vertices, int[] triangles) {
        // texture method is inside out. Finds all adjacenies, starting at the center of a hexagon. Work outward in layers.
        // Avoids assigning adjacnet verts to the same UV corrdinates for a seemless texture.
        // valid vert UV corrdinates are (0,0) , (0,1) , (1,0) and (1,1).
        adjacents = new adjacent[vertCount, 6];
        uv = new Vector2[vertCount];

        for (int i = 0; i <= vertCount - 1; i++) {
            uv[i].x = -10F;
            uv[i].y = -10F;
        }
        findAdjacents(triangles, vertCount);

        int doneVerts = 0;

        bool[] checkedMatches = new bool[3];

        int vi = 0;
        int[] vertList = new int[vertCount];
        int[] tmpVertList = new int[vertCount];
        for (int i = 0; i <= vertCount - 1; i++) { vertList[i] = -1; }

        vertList[0] = 0;
        do {
            do {
                checkedMatches = checkAdjacents(vertList[vi]);
                uv[vertList[vi]] = assignAdjacent(checkedMatches, UnityEngine.Random.Range(0, 12));
                vi += 1;
                doneVerts += 1;
            } while (vertList[vi] != -1);
            tmpVertList = vertList;
            vertList = updateVertList(vertCount, tmpVertList);
            vi = 0;
        } while (doneVerts <= vertCount - 1);

        return uv;
    }
    
    public Vector2[] AssignSplatElev(int vertCount, int parentVertCount, Vector3[] vertices) {
        // for use in the terrain shader, we'll return each vert's uv4 as either as a point along an 
        // RGB fade on the y axis or a point along an RGB fade on the X axis. 
        // In the shader we can then fade up to 6 textures. 
        Vector2[] uv4 = new Vector2[vertCount];
        float[] vertLength = new float[vertCount];
        float minElev = 650000;
        for (int i = 0; i <= vertices.Length - 1; i++) {
            vertLength[i] = (float)Math.Sqrt((vertices[i].x * vertices[i].x) +
                                             (vertices[i].y * vertices[i].y) +
                                             (vertices[i].z * vertices[i].z));
            if (vertLength[i] <= minElev) { minElev = vertLength[i]; }
            if (vertLength[i] >= maxElev) { maxElev = vertLength[i]; }
        }
        for (int i = 0; i <= vertCount - 1; i++) {
            float normalizedElev = (vertLength[i] - minElev) / (maxElev - minElev);
            uv4[i].y = normalizedElev;
            if (normalizedElev > .65F) {
                uv4[i].x = 1F;
            }
            else {
                uv4[i].x = 0F;
            }
        }
        return uv4;
    }  

    private int[] updateVertList(int vertCount, int[] oldVertList) {
        // Assign all verts that were adjacent to each vert in the old vertlist (and previously untextured).
        // Avoid dupes. Will form a new outer ring of unassigned verts. Keep track of which index we are on.
        // Use arrays because shuffling linked lists is slow and messy inside of a recursive algorithm. 
        int[] newVertList = new int[vertCount];
        for (int i = 0; i <= vertCount - 1; i++) { newVertList[i] = -1; }
        int vertIndex = 0;
        bool dupe = false;
        for (int i = 0; i <= vertCount - 1; i++) {
            if (oldVertList[i] == -1) { break; }
            for (int i2 = 0; i2 <= 5; i2++) {
                int adjVert = adjacents[oldVertList[i], i2].vert;
                if (adjVert == -99) { continue; }
                if (uv[adjVert].x == -10) {
                    for (int i3 = 0; i3 <= vertIndex; i3++) {
                        if (newVertList[i3] == adjVert) { dupe = true; }
                    }
                    if (!dupe) { newVertList[vertIndex] = adjVert; vertIndex += 1; }
                    dupe = false;
                }
            }
        }
        return newVertList;
    }
}

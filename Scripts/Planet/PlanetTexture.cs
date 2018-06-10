using UnityEngine;
using System;

public class PlanetTexture : MonoBehaviour {
    // Texture manager class, methods to texture terrain, ocean, atmosphere.
    // Assigns a tileable u,v corrdinate to each vertex. 
    // Finds the neighbors of each vert, and building from mostly hexagonal
    // primatives, we ensure that verts and their neighbors are assigned
    // some corner of a texture, and that none of the neighboring  
    // verts have the same corner assigned. Start with the Texture() Method.

    private struct adjacent {
        public int vert;
        public short count;
    }

    private adjacent[,] adjacents;
    private Vector2[] uv;
    private PlanetSplatMap splatMap;

    private bool[] checkAdjacents(int vert) {
        // checks neighboring verts, returns an array of booleans to let us know which are nearby.
        bool[] matches = new bool[4];

        if ((vert > adjacents.GetLength(0) - 1) || (vert < 0)) {
            vert = 1;
        };

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
            if (count == 6) { return; }
        }
        if (!skip3) {
            adjacents[vert1, count].vert = vert3;
            count += 1;
            if (count == 6) { return; }
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

    public Vector2[] Texture(int vertCount, Vector3[] vertices, int[] triangles, bool partialOcean = false) {
        // texture method is inside out. Finds all adjacenies, starting at the center of a hexagon. Work outward in layers.
        // Avoids assigning adjacnet verts to the same UV corrdinates for a seemless texture.
        // valid vert UV corrdinates are (0,0) , (0,1) , (1,0) and (1,1).

        if (partialOcean) {
            uv = new Vector2[vertCount];
            float maxX = -9000;
            float minX = 9000;
            float maxZ = -9000;
            float minZ = 9000;
            for (int i = 0; i <= vertCount - 1; i++) {
                if (vertices[i].x < minX) { minX = vertices[i].x; }
                if (vertices[i].x > maxX) { maxX = vertices[i].x; }
                if (vertices[i].z < minZ) { minZ = vertices[i].z; }
                if (vertices[i].z > maxZ) { maxZ = vertices[i].z; }
            }
            float xSize = Math.Abs(minX) + maxX;
            float zSize = Math.Abs(minZ) + maxZ;
            //Debug.Log("minX: " + minX);
            //Debug.Log("maxX: " + maxX);
            //Debug.Log("minZ: " + minZ);
            //Debug.Log("maxZ: " + maxZ);
            //Debug.Log("vertCount: " + vertCount);
            for (int i = 0; i <= vertCount - 1; i++) {
                uv[i] = new Vector2((vertices[i].x + Math.Abs(minX)) / xSize, (vertices[i].z + Math.Abs(minZ)) / zSize);
            }
            return uv;
        }

        System.Random rnd; int seed;
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        }
        else {
            seed = 100;
        }

        rnd = new System.Random(seed);
        
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
                if (vertList[vi] != -1) {
                    checkedMatches = checkAdjacents(vertList[vi]);
                    uv[vertList[vi]] = assignAdjacent(checkedMatches, rnd.Next(0, 12));
                    // catch the stray unassinged vertlist[vi].
                }
                vi += 1;
                doneVerts += 1;
            } while (vertList[vi] != -1);
            tmpVertList = vertList;
            vertList = updateVertList(vertCount, tmpVertList);
            vi = 0;
        } while (doneVerts <= vertCount - 1);

        // clean up and return
        tmpVertList = null; vertList = null; adjacents = null;
        return uv;
    }
    
    public Vector2[] AssignSplatElev(Vector3[] vertices, Vector3[] normals = null) {
        if (splatMap == null) { splatMap = gameObject.AddComponent<PlanetSplatMap>(); }
        return splatMap.assignSplatMap(vertices, normals);
    }

    public Vector2[] GetSplatSpecials() {
        // return uv3 from the splatMap class, which is used in the shader
        // to determine terrain type.
        return splatMap.GetUV3();
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
